namespace JobPilotAi_Backend.Modules.Analyses;

public sealed class ResumeAnalysis
{
    public Guid Id { get; private set; }

    public Guid ResumeId { get; private set; }

    public Guid UserId { get; private set; }

    public int AtsScore { get; private set; }

    public int KeywordMatchScore { get; private set; }

    public int SkillsCoverageScore { get; private set; }

    public int FormattingScore { get; private set; }

    public int ExperienceScore { get; private set; }

    public int EducationScore { get; private set; }

    public string WeaknessesJson { get; private set; } = "[]";

    public string RecommendationsJson { get; private set; } = "[]";

    public string MissingKeywordsJson { get; private set; } = "[]";

    public string StrengthsJson { get; private set; } = "[]";

    public DateTimeOffset CreatedAt { get; private set; }

    public static ResumeAnalysis Create(
        Guid resumeId,
        Guid userId,
        int keywordMatchScore,
        int skillsCoverageScore,
        int formattingScore,
        int experienceScore,
        int educationScore,
        string weaknessesJson,
        string recommendationsJson,
        string missingKeywordsJson,
        string strengthsJson,
        DateTimeOffset createdAt)
    {
        var atsScore = (int)Math.Round(
            keywordMatchScore * 0.35m
            + skillsCoverageScore * 0.25m
            + formattingScore * 0.15m
            + experienceScore * 0.15m
            + educationScore * 0.10m);

        return new ResumeAnalysis
        {
            Id = Guid.NewGuid(),
            ResumeId = resumeId,
            UserId = userId,
            AtsScore = atsScore,
            KeywordMatchScore = keywordMatchScore,
            SkillsCoverageScore = skillsCoverageScore,
            FormattingScore = formattingScore,
            ExperienceScore = experienceScore,
            EducationScore = educationScore,
            WeaknessesJson = weaknessesJson,
            RecommendationsJson = recommendationsJson,
            MissingKeywordsJson = missingKeywordsJson,
            StrengthsJson = strengthsJson,
            CreatedAt = createdAt
        };
    }
}
