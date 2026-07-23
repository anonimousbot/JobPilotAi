using JobPilotAi_Backend.Modules.Resumes;

namespace JobPilotAi_Backend.Core.Ai;

public interface IAiProvider
{
    Task<ResumeAnalysisDraft> AnalyzeResumeAsync(
        Resume resume,
        string? jobDescription,
        CancellationToken cancellationToken);

    Task<CoverLetterDraft> GenerateCoverLetterAsync(
        Resume? resume,
        string jobTitle,
        string companyName,
        string jobDescription,
        string tone,
        CancellationToken cancellationToken);
}
