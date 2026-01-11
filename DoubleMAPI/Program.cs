using AutoMapper;
using BLL.Interfaces;
using BLL.Mappings;
using BLL.Seeder;
using BLL.Services;
using BLL.Settings;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .MinimumLevel.Information()
    .CreateLogger();

try
{
    Log.Information("Starting Double M API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // Bind Configuration
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.Configure<BunnySettings>(builder.Configuration.GetSection("BunnySettings"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    builder.Services.Configure<AdminUserSettings>(builder.Configuration.GetSection("AdminUser"));

var emailSettings = builder.Configuration.GetSection("EmailSettings").Get<EmailSettings>();

builder.Services
    .AddFluentEmail(emailSettings?.SenderEmail ?? "noreply@doublem.com", emailSettings?.SenderName ?? "Double M Platform")
    .AddSmtpSender("smtp.gmail.com", 587); // Configure your SMTP settings here

    // Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis Cache
var redisConnection = builder.Configuration.GetConnectionString("RedisConnection");
if (!string.IsNullOrWhiteSpace(redisConnection))
{
    try
    {
        var options = ConfigurationOptions.Parse(redisConnection);
        options.ConnectTimeout = 2000; // 2 seconds timeout
        options.ConnectRetry = 2;
        options.AbortOnConnectFail = false; // Important: don't abort if Redis is down
        
        var redis = await ConnectionMultiplexer.ConnectAsync(options);
        
        if (redis.IsConnected)
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(redis);
            builder.Services.AddScoped<ICacheService, CacheService>();
            Log.Information("Redis cache configured successfully");
        }
        else
        {
            Log.Warning("Redis connection failed. Caching will be disabled.");
            builder.Services.AddScoped<ICacheService, NoCacheService>();
        }
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to connect to Redis. Caching will be disabled.");
        builder.Services.AddScoped<ICacheService, NoCacheService>();
    }
}
else
{
    Log.Information("No Redis connection string configured. Caching will be disabled.");
    builder.Services.AddScoped<ICacheService, NoCacheService>();
}
    // SignalR
    builder.Services.AddSignalR();

    // Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

    // Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
        options.AddPolicy("StudentOnly", policy => policy.RequireRole("Student"));
        options.AddPolicy("ParentOnly", policy => policy.RequireRole("Parent"));
        options.AddPolicy("TeacherOrAdmin", policy => policy.RequireRole("Teacher", "Admin"));
        options.AddPolicy("StudentOrParent", policy => policy.RequireRole("Student", "Parent"));
    });

    // Register Unit of Work & Services
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // BLL Services
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IDeviceSessionService, DeviceSessionService>();
    builder.Services.AddScoped<IAdminUserService, AdminUserService>();
    builder.Services.AddScoped<ICourseService, CourseService>();
    builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
    builder.Services.AddScoped<IQuizService, QuizService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IProgressService, ProgressService>();
    builder.Services.AddScoped<IParentService, ParentService>();
    builder.Services.AddScoped<ISectionService, SectionService>();
    builder.Services.AddScoped<ILessonService, LessonService>();
    builder.Services.AddScoped<ICourseCodeService, CourseCodeService>();
    builder.Services.AddScoped<ICourseAccessCodeService, CourseAccessCodeService>();
    builder.Services.AddScoped<IFileService, FileService>();
    // ✅ NEW: Register TeacherService
    builder.Services.AddScoped<ITeacherService, TeacherService>();

    // Infrastructure Services
    builder.Services.AddScoped<IEmailService, FluentEmailService>();
    builder.Services.AddHttpClient<IMediaService, MediaService>();

    // AutoMapper
    builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

    // Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Double M API",
            Version = "v1",
            Description = "Double M Educational Platform API"
        });

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
                  //.AllowCredentials();  // ✅ ADDED: Required for SignalR
        });
    });

    var app = builder.Build();

    // Middleware
    app.UseSerilogRequestLogging();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Seed Roles and Admin User
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var config = services.GetRequiredService<IConfiguration>();

        await DataSeeder.SeedRolesAndAdminAsync(
            services.GetRequiredService<RoleManager<IdentityRole>>(),
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            config
        );
    }

    // ✅ FIXED: Map SignalR Hubs (must come after authorization)
    app.MapHub<NotificationHub>("/hubs/notification");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Double M Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
