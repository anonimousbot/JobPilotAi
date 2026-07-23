using JobPilotAi_Backend.Core.Security;
using Serilog.Context;

namespace JobPilotAi_Backend.Core.Logging;

internal sealed class RequestLogContextMiddleware(
    RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext httpContext, AccessTokenService tokenService)
    {
        var correlationId = GetOrCreateCorrelationId(httpContext);
        httpContext.Response.Headers.TryAdd(CorrelationIdHeaderName, correlationId);

        var authContext = TryGetAuthContext(httpContext, tokenService);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId", authContext?.UserId))
        using (LogContext.PushProperty("UserRole", authContext?.Role.ToString()))
        {
            httpContext.Items[nameof(AuthContext)] = authContext;
            await next(httpContext);
        }
    }

    private static AuthContext? TryGetAuthContext(HttpContext httpContext, AccessTokenService tokenService)
    {
        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization)
            || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var token = authorization["Bearer ".Length..].Trim();

        return tokenService.ValidateAccessToken(token);
    }

    private static string GetOrCreateCorrelationId(HttpContext httpContext)
    {
        var existing = httpContext.Request.Headers[CorrelationIdHeaderName].ToString();

        return string.IsNullOrWhiteSpace(existing)
            ? httpContext.TraceIdentifier
            : existing;
    }
}
