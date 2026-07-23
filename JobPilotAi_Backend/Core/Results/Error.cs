namespace JobPilotAi_Backend.Core.Results;

public sealed record Error(
    string Code,
    string Description,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new("validation.failed", "One or more validation errors occurred.", ErrorType.Validation, errors);

    public static Error BadRequest(string code, string description) =>
        new(code, description, ErrorType.BadRequest);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unavailable(string code, string description) =>
        new(code, description, ErrorType.Unavailable);

    public static Error Unexpected(string code, string description) =>
        new(code, description, ErrorType.Unexpected);
}
