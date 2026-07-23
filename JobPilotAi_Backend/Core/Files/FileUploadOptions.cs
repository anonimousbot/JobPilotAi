namespace JobPilotAi_Backend.Core.Files;

public sealed class FileUploadOptions
{
    public const string SectionName = "FileUploads";

    public long MaxResumeBytes { get; init; } = 10 * 1024 * 1024;

    public string ResumeStoragePath { get; init; } = "storage/resumes";
}
