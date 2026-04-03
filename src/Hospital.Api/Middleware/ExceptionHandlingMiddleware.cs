using Hospital.Api.Contracts;
using Hospital.Application.Exceptions;

namespace Hospital.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (Exception exception)
        {
            var errorId = GetUnixTimeMicroseconds();
            var response = CreateErrorResponse(exception, errorId, out var statusCode);

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(exception, "Unhandled exception. ErrorId: {ErrorId}", errorId);
            }
            else
            {
                _logger.LogWarning(exception, "Application exception. ErrorId: {ErrorId}", errorId);
            }

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }

    private static ErrorResponse CreateErrorResponse(Exception exception, long errorId, out int statusCode)
    {
        if (exception is ApplicationExceptionBase applicationException)
        {
            statusCode = applicationException.StatusCode;

            return new ErrorResponse
            {
                Type = applicationException.ErrorType,
                Id = errorId,
                Data = new ErrorDataResponse
                {
                    Message = applicationException.Message
                }
            };
        }

        statusCode = StatusCodes.Status500InternalServerError;

        return new ErrorResponse
        {
            Type = "Exception",
            Id = errorId,
            Data = new ErrorDataResponse
            {
                Message = $"Internal server error ID = {errorId}"
            }
        };
    }

    private static long GetUnixTimeMicroseconds()
    {
        return (DateTimeOffset.UtcNow.UtcTicks - DateTimeOffset.UnixEpoch.UtcTicks) / 10;
    }
}
