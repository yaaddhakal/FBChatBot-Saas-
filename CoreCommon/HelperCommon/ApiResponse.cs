using CoreCommon.HelperCommon.Enums;
using Microsoft.AspNetCore.Http;

namespace CoreCommon.HelperCommon
{
    /// <summary>
    /// Standard API Response wrapper for all API endpoints
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }= string.Empty;
        public T? Data { get; set; }
        public List<ErrorDetail> Errors { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public string TraceId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T data, string message = "Request successful", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, ResultStatusCode statusCode = ResultStatusCode.InternalServerError, List<ErrorDetail> errors =null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = (int) statusCode,
                Message = message,
                Errors = errors ?? new List<ErrorDetail>()
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, string errorCode, ResultStatusCode statusCode = ResultStatusCode.InternalServerError)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = (int)statusCode,
                Message = message,
                Errors = new List<ErrorDetail>
                {
                    new ErrorDetail { Code = errorCode, Message = message }
                }
            };
        }
    }

    public class ErrorDetail
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public Dictionary<string, object> Details { get; set; } = new();
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResponse(string message = "Request successful", int statusCode = 200)
        {
            return new ApiResponse
            {
                Success = true,
                StatusCode = statusCode,
                Message = message
            };
        }

        public static  ApiResponse ErrorResponse(string message, ResultStatusCode statusCode = ResultStatusCode.InternalServerError)
        {
            return new ApiResponse
            {
                Success = false,
                StatusCode = (int)statusCode,
                Message = message
            };
        }
    }
}