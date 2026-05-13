using CoreCommon.HelperCommon.Enums;

public class ResultData<T>
{
    public bool Success { get; set; }
    public string Error { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public T? Data { get; set; }

    public static ResultData<T> Ok(T data, ResultStatusCode statusCode = ResultStatusCode.Ok)
    {
        return new ResultData<T>
        {
            Success = true,
            Data = data,
            StatusCode = (int)statusCode
        };
    }

    public static ResultData<T> Fail(string error, ResultStatusCode statusCode = ResultStatusCode.InternalServerError)
    {
        return new ResultData<T>
        {
            Success = false,
            Error = error,
            StatusCode = (int)statusCode
        };
    }
}