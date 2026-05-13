using CoreCommon.AuthModel.JwtSettings;
using CoreCommon.DbService;
using CoreCommon.HelperCommon;
using CoreCommon.Middleware;
using CoreCommon.Services;
using CoreCommon.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Data;
using System.Text;
using UserServiceAPI.Interfaces;
using UserServiceAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERILOG CONFIGURATION - Structured Logging
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyWallet.UserServiceAPI")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10485760,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/errors/error-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        retainedFileCountLimit: 90,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting MyWallet UserServiceAPI...");

    // ============================================================================
    // VALIDATE CONFIGURATION ON STARTUP
    // ============================================================================
    var jwtSection = builder.Configuration.GetSection("Jwt");
    if (!jwtSection.Exists())
        throw new InvalidOperationException("JWT configuration section is missing from appsettings.json");

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("DefaultConnection string is missing from appsettings.json");

    // ============================================================================
    // JWT SETTINGS CONFIGURATION
    // ============================================================================
    builder.Services.Configure<JwtSettings>(jwtSection);
    
    // ============================================================================
    // DATABASE CONNECTION - Dapper with SQL Server
    // ============================================================================
    builder.Services.AddScoped<IDbConnection>(sp =>
        new SqlConnection(connectionString));
    builder.Services.AddScoped<IDbService, DBService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
   // builder.Services.AddScoped<ClaimsHelper>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    // ============================================================================
    // AUTHENTICATION & AUTHORIZATION - JWT Bearer
    // ============================================================================
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = jwtSection.Get<JwtSettings>();

        if (jwtSettings == null)
            throw new InvalidOperationException("JWT settings are not configured in appsettings.json");

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
            throw new InvalidOperationException("JWT Key is missing in appsettings.json");

        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
            throw new InvalidOperationException("JWT Issuer is missing in appsettings.json");

        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
            throw new InvalidOperationException("JWT Audience is missing in appsettings.json");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Log.Debug("JWT token validated for user: {UserId}", userId);
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // ============================================================================
    // CONTROLLERS & API CONFIGURATION
    // ============================================================================
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = false;
        });

    // ============================================================================
    // CORS CONFIGURATION
    // ============================================================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // ============================================================================
    // SWAGGER/OpenAPI CONFIGURATION
    // ============================================================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "MyWallet User Service API",
            Version = "v1",
            Description = "User Service API",
            Contact = new OpenApiContact
            {
                Name = "Your Company",
                Email = "support@yourcompany.com"
            }
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme."
        });

        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key. Format: 'X-Api-Key: {your_api_key}'",
            In = ParameterLocation.Header,
            Name = "X-Api-Key",
            Type = SecuritySchemeType.ApiKey
        });

        // Apply security globally
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        Array.Empty<string>()
    },
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "ApiKey"
            }
        },
        Array.Empty<string>()
    }
});
    });

    // ============================================================================
    // HEALTH CHECKS
    // ============================================================================
    builder.Services.AddHealthChecks();

    // ============================================================================
    // BUILD APPLICATION
    // ============================================================================
    var app = builder.Build();

    // ============================================================================
    // MIDDLEWARE PIPELINE
    // ============================================================================

    // 1. Global Exception Handler
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // 2. Request Logging
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 3. Swagger (Development only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyWallet User Service API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    // 4. HTTPS Redirection
    app.UseHttpsRedirection();

    // 5. Routing
    app.UseRouting();

    // 6. CORS
    app.UseCors("AllowAll");

    // 7. API Key Middleware
    app.UseMiddleware<ApiKeyMiddleware>();

    // 8. Authentication
    app.UseAuthentication();

    // 9. Authorization
    app.UseAuthorization();

    // 10. Controllers
    app.MapControllers();

    // 11. Health Check
    app.MapHealthChecks("/health");

    // ============================================================================
    // RUN APPLICATION
    // ============================================================================
    Log.Information("MyWallet UserServiceAPI started successfully");
    Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);

    var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
    lifetime.ApplicationStarted.Register(() =>
    {
        Log.Information("Listening on: {Urls}", string.Join(", ", app.Urls));
    });

    app.Run();
}
catch (Exception ex)
{
    if (ex is AggregateException aggEx)
    {
        foreach (var inner in aggEx.InnerExceptions)
            Log.Fatal(inner, "Inner exception during startup");
    }
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}