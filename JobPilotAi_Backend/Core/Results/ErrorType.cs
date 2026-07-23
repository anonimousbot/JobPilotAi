namespace JobPilotAi_Backend.Core.Results;

public enum ErrorType
{
    BadRequest,
    Validation,
    Unauthorized,
    Forbidden,
    NotFound,
    Conflict,
    Unavailable,
    Unexpected
}
