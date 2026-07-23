namespace JobPilotAi_Backend.Features.Analyses;

public sealed record CreateAnalysisRequest(string ResumeId, string? JobDescription);

public sealed record AnalysisResponse(
    Guid Id,
    Guid ResumeId,
    int AtsScore,
    int KeywordMatchScore,
    int SkillsCoverageScore,
    int FormattingScore,
    int ExperienceScore,
    int EducationScore,
    IReadOnlyCollection<string> Weaknesses,
    IReadOnlyCollection<string> Recommendations,
    IReadOnlyCollection<string> MissingKeywords,
    IReadOnlyCollection<string> Strengths,
    DateTimeOffset CreatedAt);
