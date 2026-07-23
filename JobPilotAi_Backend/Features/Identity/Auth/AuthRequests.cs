namespace JobPilotAi_Backend.Features.Identity.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record VerifyEmailRequest(string Email);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Email, string NewPassword);

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    bool EmailConfirmed,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt);
