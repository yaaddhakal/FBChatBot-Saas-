using CoreCommon.HelperCommon;
using CoreCommon.HelperCommon.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreCommon.Middleware
{
    /// <summary>
    /// Central error handler - catches ALL unhandled exceptions from the entire pipeline.
    /// No try/catch needed in repositories or controllers - everything bubbles up here.
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = Guid.NewGuid().ToString();

            _logger.LogError(exception,
                "Unhandled exception. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
                traceId, context.Request.Path, context.Request.Method);

            var (statusCode, message, errorCode) = GetErrorDetails(exception);

            var response = ApiResponse<object>.ErrorResponse(message, errorCode, ResultStatusCode.InternalServerError);
            response.TraceId = traceId;

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                response.Metadata["StackTrace"] = exception.StackTrace;
                response.Metadata["InnerException"] = exception.InnerException?.Message;
                response.Metadata["ExceptionType"] = exception.GetType().Name;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }

        private (int statusCode, string message, string errorCode) GetErrorDetails(Exception exception)
        {
            return exception switch
            {
                UnauthorizedAccessException =>
                    ((int)HttpStatusCode.Unauthorized, "Unauthorized access", "AUTH_ERROR"),

                ArgumentNullException argEx =>
                    ((int)HttpStatusCode.BadRequest, $"Missing required parameter: {argEx.ParamName}", "VALIDATION_ERROR"),

                ArgumentException =>
                    ((int)HttpStatusCode.BadRequest, exception.Message, "VALIDATION_ERROR"),

                InvalidOperationException =>
                    ((int)HttpStatusCode.BadRequest, exception.Message, "INVALID_OPERATION"),

                KeyNotFoundException =>
                    ((int)HttpStatusCode.NotFound, "Resource not found", "NOT_FOUND"),

                TimeoutException =>
                    ((int)HttpStatusCode.RequestTimeout, "Request timeout", "TIMEOUT"),

                // SQL/DB exceptions will fall here as generic 500
                _ => ((int)HttpStatusCode.InternalServerError,
                      "An internal server error occurred. Please contact support.",
                      "INTERNAL_ERROR")
            };
        }
    }
}
