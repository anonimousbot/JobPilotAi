namespace JobPilotAi_Backend.Modules.JobMatches;

public sealed class SavedJobMatch
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string JobMatchId { get; private set; } = string.Empty;

    public string JobTitle { get; private set; } = string.Empty;

    public string CompanyName { get; private set; } = string.Empty;

    public string Location { get; private set; } = string.Empty;

    public int MatchScore { get; private set; }

    public string TagsJson { get; private set; } = "[]";

    public DateTimeOffset SavedAt { get; private set; }

    public static SavedJobMatch Create(
        Guid userId,
        string jobMatchId,
        string jobTitle,
        string companyName,
        string location,
        int matchScore,
        string tagsJson,
        DateTimeOffset savedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            JobMatchId = jobMatchId.Trim(),
            JobTitle = jobTitle.Trim(),
            CompanyName = companyName.Trim(),
            Location = location.Trim(),
            MatchScore = matchScore,
            TagsJson = tagsJson,
            SavedAt = savedAt
        };
}
