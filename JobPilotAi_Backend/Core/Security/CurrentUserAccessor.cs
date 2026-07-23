using JobPilotAi_Backend.Core.Results;

namespace JobPilotAi_Backend.Core.Security;

public sealed class CurrentUserAccessor(
    IHttpContextAccessor httpContextAccessor,
    AccessTokenService tokenService)
{
    public Result<AuthContext> GetRequiredUser()
    {
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)
            || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AuthContext>.Failure(
                Error.Unauthorized("auth.missing_token", "A bearer access token is required."));
        }

        var token = authorization["Bearer ".Length..].Trim();
        var authContext = tokenService.ValidateAccessToken(token);

        return authContext is null
            ? Result<AuthContext>.Failure(Error.Unauthorized("auth.invalid_token", "The access token is invalid or expired."))
            : Result<AuthContext>.Success(authContext);
    }
}
