using BLL.Interfaces;
using BLL.Services;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using DAL.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Configure Serilog early so startup logs are captured
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .CreateLogger();

// Hook Serilog into the generic host and ensure it reads configuration as well
builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId();
});

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
        ValidIssuer = jwtSettings.Issuer,
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

builder.Services.AddScoped<IEmailService, FluentEmailService>();


// Register Fluent Email service
builder.Services.AddFluentEmail(
        "no.reply@double-m.com",
        "Double M"
    )
    .AddSmtpSender(() =>
    {
        return new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(
                "doublemplatform@gmail.com",
                "Ahmed@21"
            ),
            EnableSsl = true
        };
    });

// Register Unit of Work (BLL will use this)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();



// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

try
{
    Log.Information("Starting web host");

    var app = builder.Build();


    // logs HTTP requests
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "Handled {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    //Exception Handling Middleware
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exception = context.Features
                .Get<IExceptionHandlerFeature>()?.Error;

            Log.Error(exception, "Unhandled exception");

            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Unexpected error"
            });
        });
    });

    // Configure the HTTP request pipeline.
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

    // Seed Roles
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedRolesAsync(roleManager);
    }


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roles = { "Admin", "Teacher", "Student", "Parent" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Log.Information("Role {Role} created", role);
        }
    }
}
