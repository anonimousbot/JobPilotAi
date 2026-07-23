namespace JobPilotAi_Backend.Features.JobMatches;

public sealed record JobMatchSearchRequest(
    string? Query,
    string? Location,
    string? ResumeId);

public sealed record JobMatchResponse(
    string Id,
    string JobTitle,
    string CompanyName,
    string Location,
    int MatchScore,
    IReadOnlyCollection<string> Tags,
    string Summary,
    bool Saved);

public sealed record SavedJobMatchResponse(
    Guid Id,
    string JobMatchId,
    string JobTitle,
    string CompanyName,
    string Location,
    int MatchScore,
    IReadOnlyCollection<string> Tags,
    DateTimeOffset SavedAt);

public sealed record JobMatchComparisonResponse(
    string JobMatchId,
    string JobTitle,
    string CompanyName,
    int MatchScore,
    IReadOnlyCollection<string> RequiredSkills,
    IReadOnlyCollection<string> MatchedSkills,
    IReadOnlyCollection<string> MissingSkills,
    IReadOnlyCollection<string> Recommendations,
    string Insight);
