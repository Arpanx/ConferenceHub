using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHub.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BusinessException businessException)
        {
            httpContext.Response.StatusCode =
                businessException.StatusCode;

            var problemDetails = new ProblemDetails
            {
                Status = businessException.StatusCode,
                Title = "Business error",
                Detail = businessException.Message
            };

            problemDetails.Extensions["code"] =
                businessException.ErrorCode;

            await httpContext.Response.WriteAsJsonAsync(
                problemDetails,
                cancellationToken);

            return true;
        }

        _logger.LogError(
            exception,
            "Unhandled exception occurred.");

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var serverError = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal server error",
            Detail = "An unexpected error occurred."
        };

        await httpContext.Response.WriteAsJsonAsync(
            serverError,
            cancellationToken);

        return true;
    }
}