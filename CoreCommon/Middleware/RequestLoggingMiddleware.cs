using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CoreCommon.Middleware
{
    /// <summary>
    /// Middleware to log all HTTP requests and responses
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var traceId = Guid.NewGuid().ToString();
            context.Items["TraceId"] = traceId;

            // Start timing
            var stopwatch = Stopwatch.StartNew();

            // Log request
            await LogRequest(context, traceId);

            // Capture the response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log response
                await LogResponse(context, traceId, stopwatch.ElapsedMilliseconds);

                // Copy response back to original stream
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
            }
        }

        private async Task LogRequest(HttpContext context, string traceId)
        {
            var request = context.Request;
            
            // Log basic request info
            _logger.LogInformation(
                "HTTP {Method} {Path} started. TraceId: {TraceId}, IP: {IP}, User: {User}",
                request.Method,
                request.Path,
                traceId,
                context.Connection.RemoteIpAddress,
                context.User?.Identity?.Name ?? "Anonymous"
            );

            // Optionally log request body for POST/PUT (be careful with sensitive data)
            if ((request.Method == "POST" || request.Method == "PUT") && request.ContentLength > 0)
            {
                request.EnableBuffering();
                var buffer = new byte[Convert.ToInt32(request.ContentLength)];
                await request.Body.ReadAsync(buffer, 0, buffer.Length);
                var bodyAsText = Encoding.UTF8.GetString(buffer);
                request.Body.Position = 0;

                // Mask sensitive fields (password, etc.)
                var maskedBody = MaskSensitiveData(bodyAsText);
                _logger.LogDebug("Request Body: {Body}", maskedBody);
            }
        }

        private async Task LogResponse(HttpContext context, string traceId, long elapsedMs)
        {
            var response = context.Response;
            
            _logger.LogInformation(
                "HTTP {Method} {Path} completed. TraceId: {TraceId}, Status: {StatusCode}, Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                traceId,
                response.StatusCode,
                elapsedMs
            );

            // Log slow requests
            if (elapsedMs > 3000) // 3 seconds
            {
                _logger.LogWarning(
                    "Slow request detected. {Method} {Path} took {Duration}ms. TraceId: {TraceId}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsedMs,
                    traceId
                );
            }
        }

        private string MaskSensitiveData(string body)
        {
            // Mask common sensitive fields
            var sensitiveFields = new[] { "password", "token", "apikey", "secret" };
            
            foreach (var field in sensitiveFields)
            {
                // Simple masking - in production, use proper JSON parsing
                if (body.Contains($"\"{field}\"", StringComparison.OrdinalIgnoreCase))
                {
                    body = System.Text.RegularExpressions.Regex.Replace(
                        body,
                        $@"(""{field}""\s*:\s*"")[^""]*""",
                        "$1***MASKED***\"",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                }
            }
            
            return body;
        }
    }
}
