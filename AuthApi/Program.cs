

using AuthAPI.Function.Services;
using AuthAPI.Interfaces;
using AuthAPI.Repositories;
using CoreCommon.AuthModel.JwtSettings;
using CoreCommon.DbService;
using CoreCommon.Middleware;
using CoreCommon.Services;
using CoreCommon.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Data;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// SERILOG CONFIGURATION - Structured Logging
// ============================================================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "MyWallet.AuthAPI")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        fileSizeLimitBytes: 10485760, // 10MB
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
    Log.Information("Starting MyWallet AuthAPI...");

    // ============================================================================
    // JWT SETTINGS CONFIGURATION
    // ============================================================================
    
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    builder.Services.AddScoped<JwtService>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<ITokenRepository, TokenRepository>();
    builder.Services.AddScoped<ILogInRepository, LogInRepository>();
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
    // ============================================================================
    // DATABASE CONNECTION - Dapper with SQL Server
    // ============================================================================
    builder.Services.AddScoped<IDbConnection>(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));
    builder.Services.AddScoped <IDbService, DBService>();
    






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
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
        
        if (jwtSettings == null)
        {
            throw new InvalidOperationException("JWT settings are not configured in appsettings.json");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // Remove default 5-minute tolerance
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        // Log JWT validation events
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
    // DEPENDENCY INJECTION - Services & Repositories
    // ============================================================================
   
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
    // CORS CONFIGURATION (if needed)
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
            Title = "MyWallet Auth API",
            Version = "v1",
            Description = "Authentication API with API Key and JWT Bearer token security",
            Contact = new OpenApiContact
            {
                Name = "Your Company",
                Email = "support@yourcompany.com"
            },
        });
        // Register the custom filter
        c.OperationFilter<ResultDataResponseFilter>();


        // API Key Security Definition
        c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
        {
            Description = "API Key required for all endpoints. Format: 'X-Api-Key: {your_api_key}'",
            In = ParameterLocation.Header,
            Name = "X-Api-Key",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "ApiKeyScheme"
        });

        // JWT Bearer Security Definition
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Description = "JWT Authorization header using the Bearer scheme."
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
    
    // 1. Global Exception Handler (should be first)
    app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

    // 2. Request Logging
    app.UseMiddleware<RequestLoggingMiddleware>();

    // 3. Swagger (Development only)
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyWallet Auth API v1");
            c.RoutePrefix = string.Empty; // Swagger at root
        });
        
        Log.Information("Swagger UI available at: http://localhost:{Port}/", 
            app.Urls.FirstOrDefault()?.Split(':').Last());
    }

    // 4. HTTPS Redirection
    app.UseHttpsRedirection();

    // 5. Routing
    app.UseRouting();

    // 6. CORS (if needed)
    app.UseCors("AllowAll");

    // 7. API Key Middleware (before authentication)
    app.UseMiddleware<ApiKeyMiddleware>();

    // 8. Authentication (JWT validation)
    app.UseAuthentication();

    // 9. Authorization (Claims checking)
    app.UseAuthorization();

    // 10. Map Controllers
    app.MapControllers();

    // 11. Health Check Endpoint
    app.MapHealthChecks("/health");

    // ============================================================================
    // RUN APPLICATION
    // ============================================================================
    Log.Information("MyWallet AuthAPI started successfully");
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
        {
            Log.Fatal(inner, "Inner exception during startup");
        }
    }

    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
