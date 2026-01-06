using AutoMapper;
using BLL.Interfaces;
using BLL.Mappings;
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
using System.Net;
using System.Net.Mail;
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

    // Configuration Binding
    builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
    builder.Services.Configure<BunnySettings>(builder.Configuration.GetSection("BunnySettings"));
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

    // Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false; // Set to true in production
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

    // Register Unit of Work
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Register Business Logic Services
    builder.Services.AddScoped<ICourseService, CourseService>();
    builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
    builder.Services.AddScoped<IQuizService, QuizService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IProgressService, ProgressService>();
    builder.Services.AddScoped<IParentService, ParentService>();
    builder.Services.AddScoped<ISectionService, SectionService>();
    builder.Services.AddScoped<ILessonService, LessonService>();
    builder.Services.AddScoped<ICourseCodeService, CourseCodeService>();
    builder.Services.AddScoped<IFileService, FileService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();


    // Register Infrastructure Services
    builder.Services.AddScoped<IEmailService, FluentEmailService>();
    builder.Services.AddHttpClient<IMediaService, MediaService>();

    // AutoMapper
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.AddProfile<MappingProfile>();
    });


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

        // Add JWT Authentication to Swagger
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
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedRolesAndAdminAsync(roleManager, userManager);
    }

    Log.Information("Double M API started successfully");
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

async Task SeedRolesAndAdminAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    try
    {
        // Seed Roles
        string[] roles = { "Admin", "Teacher", "Student", "Parent" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Log.Information("Role {Role} created", role);
            }
        }

        // Seed Default Admin User
        var adminEmail = "admin@doublem.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Double M Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123456");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Log.Information("Default admin user created: {Email}", adminEmail);
            }
            else
            {
                Log.Error("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error seeding roles and admin user");
    }
}