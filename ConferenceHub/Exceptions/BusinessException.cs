namespace ConferenceHub.Exceptions;

public class BusinessException : Exception
{
    public int StatusCode { get; }

    public string ErrorCode { get; }

    public BusinessException(
        string message,
        string errorCode = "BUSINESS_ERROR",
        int statusCode = StatusCodes.Status400BadRequest)
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}