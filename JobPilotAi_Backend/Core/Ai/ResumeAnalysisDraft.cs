namespace JobPilotAi_Backend.Core.Ai;

public sealed record ResumeAnalysisDraft(
    int KeywordMatchScore,
    int SkillsCoverageScore,
    int FormattingScore,
    int ExperienceScore,
    int EducationScore,
    string WeaknessesJson,
    string RecommendationsJson,
    string MissingKeywordsJson,
    string StrengthsJson,
    AiUsageMetrics Usage);
