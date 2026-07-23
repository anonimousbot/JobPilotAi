namespace JobPilotAi_Backend.Features.Resumes;

public sealed record UploadResumeRequest(IFormFile File);

public sealed record ResumeResponse(
    Guid Id,
    string FileName,
    string FileType,
    long FileSize,
    string ContentHash,
    DateTimeOffset UploadedAt);

public sealed record UploadResumeResponse(
    Guid Id,
    string FileName,
    string FileType,
    long FileSize,
    bool DuplicateFound,
    DateTimeOffset UploadedAt);
