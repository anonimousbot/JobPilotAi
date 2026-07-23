namespace JobPilotAi_Backend.Modules.Resumes;

public sealed class Resume
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string FileType { get; private set; } = string.Empty;

    public long FileSize { get; private set; }

    public string StoragePath { get; private set; } = string.Empty;

    public string ContentHash { get; private set; } = string.Empty;

    public string? ExtractedText { get; private set; }

    public string SkillsJson { get; private set; } = "[]";

    public string ExperienceJson { get; private set; } = "[]";

    public string EducationJson { get; private set; } = "[]";

    public DateTimeOffset UploadedAt { get; private set; }

    public static Resume Create(
        Guid userId,
        string fileName,
        string fileType,
        long fileSize,
        string storagePath,
        string contentHash,
        string? extractedText,
        DateTimeOffset uploadedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FileName = fileName.Trim(),
            FileType = fileType,
            FileSize = fileSize,
            StoragePath = storagePath,
            ContentHash = contentHash,
            ExtractedText = extractedText,
            UploadedAt = uploadedAt
        };
}
