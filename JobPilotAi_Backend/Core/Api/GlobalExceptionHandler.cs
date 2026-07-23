using Microsoft.AspNetCore.Diagnostics;
using JobPilotAi_Backend.Core.Results;
using System.Text.Json;

namespace JobPilotAi_Backend.Core.Api;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is BadHttpRequestException or JsonException)
        {
            logger.LogWarning(exception, "Bad request or JSON deserialization failure occurred.");

            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            httpContext.Response.ContentType = "application/json";

            var response = new ApiErrorResponse(false, ["Invalid request payload or malformed JSON input. Please check your data formatting."]);
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }

        logger.LogError(exception, "An unhandled exception occurred during request execution.");

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var serverErrorResponse = new ApiErrorResponse(false, ["An unexpected error occurred while processing your request."]);
        await httpContext.Response.WriteAsJsonAsync(serverErrorResponse, cancellationToken);
        return true;
    }
}
