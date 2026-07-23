namespace JobPilotAi_Backend.Features.Profiles;

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Location,
    string? LinkedInUrl,
    string? PortfolioUrl,
    string? CareerGoal,
    string? TargetRole,
    string? CareerLevel);

public sealed record UpdateOnboardingProfileRequest(
    string CareerGoal,
    string TargetRole,
    string CareerLevel,
    string? CurrentJobTitle,
    int? YearsOfExperience);

public sealed record ProfileResponse(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Location,
    string? LinkedInUrl,
    string? PortfolioUrl,
    string? CareerGoal,
    string? TargetRole,
    string? CareerLevel,
    DateTimeOffset UpdatedAt);
