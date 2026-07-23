namespace JobPilotAi_Backend.Modules.CoverLetters;

public sealed class CoverLetter
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public Guid? ResumeId { get; private set; }

    public string JobTitle { get; private set; } = string.Empty;

    public string CompanyName { get; private set; } = string.Empty;

    public string JobDescription { get; private set; } = string.Empty;

    public string Tone { get; private set; } = "Professional";

    public string OriginalContent { get; private set; } = string.Empty;

    public string EditedContent { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public static CoverLetter Create(
        Guid userId,
        Guid? resumeId,
        string jobTitle,
        string companyName,
        string jobDescription,
        string tone,
        string content,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ResumeId = resumeId,
            JobTitle = jobTitle.Trim(),
            CompanyName = companyName.Trim(),
            JobDescription = jobDescription.Trim(),
            Tone = tone.Trim(),
            OriginalContent = content,
            EditedContent = content,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

    public void Edit(string editedContent, DateTimeOffset updatedAt)
    {
        EditedContent = editedContent.Trim();
        UpdatedAt = updatedAt;
    }

    public void Regenerate(string content, DateTimeOffset updatedAt)
    {
        OriginalContent = content;
        EditedContent = content;
        UpdatedAt = updatedAt;
    }
}
