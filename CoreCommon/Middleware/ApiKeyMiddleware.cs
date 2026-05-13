using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreCommon.Middleware
{
    /// <summary>
    /// Enhanced API Key middleware with selective endpoint protection
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiKeyMiddleware> _logger;
        private const string API_KEY_HEADER = "X-Api-Key";

        // Endpoints that don't require API key (if needed)
        private readonly string[] _excludedPaths = new[]
        {
            "/health",
            "/swagger"
        };

        public ApiKeyMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<ApiKeyMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Skip API key check for excluded paths
            if (_excludedPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            } 

            // Check for API key in header
            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedApiKey))
            {
                _logger.LogWarning("API Key missing from request. Path: {Path}, IP: {IP}", 
                    context.Request.Path, 
                    context.Connection.RemoteIpAddress);

                await WriteUnauthorizedResponse(context, "API Key is missing");
                return;
            }

            // Validate API key
            var validApiKey = _configuration["ApiKey"];
            if (string.IsNullOrEmpty(validApiKey))
            {
                _logger.LogError("API Key not configured in appsettings.json");
                await WriteUnauthorizedResponse(context, "API Key configuration error");
                return;
            }

            if (!validApiKey.Equals(extractedApiKey.ToString()))
            {
                _logger.LogWarning("Invalid API Key attempt. Path: {Path}, IP: {IP}", 
                    context.Request.Path, 
                    context.Connection.RemoteIpAddress);

                await WriteUnauthorizedResponse(context, "Invalid API Key");
                return;
            }

            // API key is valid, continue to next middleware
            _logger.LogDebug("Valid API Key provided for {Path}", context.Request.Path);
            await _next(context);
        }

        private async Task WriteUnauthorizedResponse(HttpContext context, string message)
        {
            var response = ApiResponse.ErrorResponse(
                message: message,
                errorCode: "INVALID_API_KEY", ResultStatusCode.InternalServerError

            );

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 401;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonResponse = JsonSerializer.Serialize(response, jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
