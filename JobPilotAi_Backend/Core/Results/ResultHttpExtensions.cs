namespace JobPilotAi_Backend.Core.Results;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        var error = result.Error ?? Error.Unexpected("result.missing_error", "The operation failed.");

        return error.Type switch
        {
            ErrorType.BadRequest => ErrorResponse(StatusCodes.Status400BadRequest, error.Description),
            ErrorType.Validation => ErrorResponse(StatusCodes.Status400BadRequest, FlattenValidationErrors(error)),
            ErrorType.Unauthorized => ErrorResponse(StatusCodes.Status401Unauthorized, error.Description),
            ErrorType.Forbidden => ErrorResponse(StatusCodes.Status403Forbidden, error.Description),
            ErrorType.NotFound => ErrorResponse(StatusCodes.Status404NotFound, error.Description),
            ErrorType.Conflict => ErrorResponse(StatusCodes.Status409Conflict, error.Description),
            ErrorType.Unavailable => ErrorResponse(StatusCodes.Status503ServiceUnavailable, error.Description),
            _ => ErrorResponse(StatusCodes.Status500InternalServerError, error.Description)
        };
    }

    private static IResult ErrorResponse(int statusCode, string error) =>
        TypedResults.Json(new ApiErrorResponse(false, [error]), statusCode: statusCode);

    private static IResult ErrorResponse(int statusCode, IReadOnlyCollection<string> errors) =>
        TypedResults.Json(new ApiErrorResponse(false, errors), statusCode: statusCode);

    private static IReadOnlyCollection<string> FlattenValidationErrors(Error error) =>
        error.ValidationErrors?.Values.SelectMany(messages => messages).Distinct().ToArray()
        ?? [error.Description];
}

public sealed record ApiErrorResponse(bool Success, IReadOnlyCollection<string> Errors);
