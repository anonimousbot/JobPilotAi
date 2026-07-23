namespace JobPilotAi_Backend.Features.Identity.Register;

public sealed record RegisterUserRequest(
    string Email,
    string Password);
