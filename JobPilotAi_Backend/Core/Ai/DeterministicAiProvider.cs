using System.Diagnostics;
using System.Text.Json;
using JobPilotAi_Backend.Modules.Resumes;

namespace JobPilotAi_Backend.Core.Ai;

public sealed class DeterministicAiProvider : IAiProvider
{
    public Task<ResumeAnalysisDraft> AnalyzeResumeAsync(
        Resume resume,
        string? jobDescription,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var fileSignal = Math.Clamp((int)(resume.FileSize / 1024), 1, 100);
        var hasJobDescription = !string.IsNullOrWhiteSpace(jobDescription);

        var keyword = hasJobDescription ? 72 : 58;
        var skills = Math.Clamp(55 + fileSignal / 3, 55, 88);
        var formatting = string.Equals(resume.FileType, ".pdf", StringComparison.OrdinalIgnoreCase) ? 82 : 76;
        var experience = hasJobDescription ? 70 : 60;
        var education = 65;

        stopwatch.Stop();
        var draft = new ResumeAnalysisDraft(
            keyword,
            skills,
            formatting,
            experience,
            education,
            JsonSerializer.Serialize(new[] { "Add more role-specific impact metrics.", "Mirror important keywords from the job description." }),
            JsonSerializer.Serialize(new[] { "Lead each role with measurable outcomes.", "Keep formatting simple for ATS parsing." }),
            JsonSerializer.Serialize(hasJobDescription ? new[] { "domain keywords", "tooling keywords" } : Array.Empty<string>()),
            JsonSerializer.Serialize(new[] { "Readable file format", "Clear application-ready structure" }),
            new AiUsageMetrics(450, Math.Max(1, (int)stopwatch.ElapsedMilliseconds), 0.0025m));

        return Task.FromResult(draft);
    }

    public Task<CoverLetterDraft> GenerateCoverLetterAsync(
        Resume? resume,
        string jobTitle,
        string companyName,
        string jobDescription,
        string tone,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var resumeLine = resume is null
            ? "I would bring focused execution, clear communication, and strong ownership to the role."
            : $"My resume background aligns with the {jobTitle} role through relevant experience and practical delivery.";

        var content = $"""
            Dear {companyName} Hiring Team,

            I am excited to apply for the {jobTitle} role at {companyName}. {resumeLine}

            Your job description points to a need for someone who can understand priorities quickly, communicate clearly, and turn requirements into dependable results. I would welcome the opportunity to contribute that mindset in a {tone.ToLowerInvariant()} and collaborative way.

            Thank you for your time and consideration.
            """;

        stopwatch.Stop();
        return Task.FromResult(new CoverLetterDraft(
            content,
            new AiUsageMetrics(650, Math.Max(1, (int)stopwatch.ElapsedMilliseconds), 0.0035m)));
    }
}
