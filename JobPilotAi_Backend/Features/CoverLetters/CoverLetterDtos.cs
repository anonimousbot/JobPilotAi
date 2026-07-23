namespace JobPilotAi_Backend.Features.CoverLetters;

public sealed record CreateCoverLetterRequest(
    Guid? ResumeId,
    string JobTitle,
    string CompanyName,
    string JobDescription,
    string? Tone);

public sealed record UpdateCoverLetterRequest(string EditedContent);

public sealed record CoverLetterResponse(
    Guid Id,
    Guid? ResumeId,
    string JobTitle,
    string CompanyName,
    string JobDescription,
    string Tone,
    string OriginalContent,
    string EditedContent,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
