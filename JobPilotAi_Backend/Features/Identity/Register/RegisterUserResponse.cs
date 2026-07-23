namespace JobPilotAi_Backend.Features.Identity.Register;

public sealed record RegisterUserResponse(
    Guid Id,
    string Email,
    string Role,
    bool EmailConfirmed,
    DateTimeOffset CreatedAt);
