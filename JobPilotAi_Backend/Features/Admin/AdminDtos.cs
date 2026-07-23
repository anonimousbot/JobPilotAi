namespace JobPilotAi_Backend.Features.Admin;

public sealed record AdminUserResponse(
    Guid Id,
    string Email,
    string Role,
    bool EmailConfirmed,
    string Status,
    DateTimeOffset CreatedAt);

public sealed record AdminAnalyticsResponse(
    int Users,
    int Resumes,
    int Analyses,
    int CoverLetters,
    int ActiveSubscriptions,
    decimal EstimatedAiSpend);

public sealed record AdminLogResponse(
    Guid Id,
    Guid AdminId,
    string Action,
    string TargetEntity,
    Guid? TargetId,
    string MetadataJson,
    DateTimeOffset CreatedAt);
