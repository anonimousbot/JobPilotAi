using System.Text.Json;
using FluentValidation;
using JobPilotAi_Backend.Core.Ai;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Usage;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Analyses;
using JobPilotAi_Backend.Modules.Billing;
using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Hosting;

namespace JobPilotAi_Backend.Features.Analyses;

public static class AnalysisFeature
{
    public static IEndpointRouteBuilder MapAnalysisFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analyses").WithTags("ATS Analysis");

        group.MapPost("", CreateAsync)
            .WithName("CreateAnalysis")
            .WithSummary("Generates an ATS analysis for a resume.");

        group.MapGet("", ListAsync)
            .WithName("ListAnalyses")
            .WithSummary("Lists ATS analyses for the authenticated user.");

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetAnalysis")
            .WithSummary("Gets one ATS analysis for the authenticated user.");

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateAnalysisRequest request,
        IValidator<CreateAnalysisRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        IAiProvider aiProvider,
        QuotaService quotaService,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory,
        IWebHostEnvironment env,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<AnalysisResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var resumeId = Guid.Parse(request.ResumeId);

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var user = await dbContext.Users.FindAsync([auth.Value!.UserId], cancellationToken);
        if (user?.EmailConfirmed != true)
        {
            return Result<AnalysisResponse>.Failure(Error.Forbidden("auth.email_not_verified", "Email verification is required before ATS analysis.")).ToHttpResult();
        }

        var resume = await dbContext.Resumes.SingleOrDefaultAsync(
            item => item.Id == resumeId && item.UserId == auth.Value.UserId,
            cancellationToken);
        if (resume is null)
        {
            return Result<AnalysisResponse>.Failure(Error.NotFound("resume.not_found", "The resume was not found.")).ToHttpResult();
        }

        if (string.IsNullOrWhiteSpace(resume.ExtractedText))
        {
            try
            {
                var absolutePath = Path.Combine(env.ContentRootPath, resume.StoragePath);
                if (File.Exists(absolutePath))
                {
                    var bytes = await File.ReadAllBytesAsync(absolutePath, cancellationToken);
                    var extension = Path.GetExtension(resume.FileName).ToLowerInvariant();
                    string? extracted = null;
                    if (extension == ".pdf")
                    {
                        using var document = UglyToad.PdfPig.PdfDocument.Open(bytes);
                        var sb = new global::System.Text.StringBuilder();
                        foreach (var page in document.GetPages())
                        {
                            sb.AppendLine(page.Text);
                        }
                        extracted = sb.ToString().Trim();
                    }
                    else if (extension == ".docx")
                    {
                        using var memoryStream = new global::System.IO.MemoryStream(bytes);
                        using var archive = new global::System.IO.Compression.ZipArchive(memoryStream);
                        var entry = archive.GetEntry("word/document.xml");
                        if (entry is not null)
                        {
                            using var reader = new global::System.IO.StreamReader(entry.Open());
                            var xml = reader.ReadToEnd();
                            var sb = new global::System.Text.StringBuilder();
                            var matches = global::System.Text.RegularExpressions.Regex.Matches(xml, @"<w:t[^>]*>(.*?)</w:t>");
                            foreach (global::System.Text.RegularExpressions.Match match in matches)
                            {
                                sb.Append(match.Groups[1].Value).Append(' ');
                            }
                            extracted = global::System.Net.WebUtility.HtmlDecode(sb.ToString().Trim());
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(extracted))
                    {
                        dbContext.Entry(resume).Property(r => r.ExtractedText).CurrentValue = extracted;
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                loggerFactory.CreateLogger("ResumeParser").LogWarning(ex, "Failed to parse existing resume file {ResumeId}", resume.Id);
            }
        }

        var quota = await quotaService.EnsureCanUseAsync(auth.Value.UserId, UsageActionType.AtsAnalysis, cancellationToken);
        if (!quota.IsSuccess)
        {
            return quota.ToHttpResult();
        }

        ResumeAnalysisDraft draft;
        try
        {
            draft = await aiProvider.AnalyzeResumeAsync(resume, request.JobDescription, cancellationToken);
        }
        catch (Exception exception)
        {
            loggerFactory.CreateLogger<OpenAiProvider>().LogWarning(
                exception,
                "ATS analysis provider failed. UserId: {UserId}, ResumeId: {ResumeId}",
                auth.Value.UserId,
                resume.Id);

            return Result<AnalysisResponse>
                .Failure(Error.Unavailable("ai.provider_unavailable", "The AI provider is currently unavailable."))
                .ToHttpResult();
        }

        var analysis = ResumeAnalysis.Create(
            resume.Id,
            auth.Value.UserId,
            draft.KeywordMatchScore,
            draft.SkillsCoverageScore,
            draft.FormattingScore,
            draft.ExperienceScore,
            draft.EducationScore,
            draft.WeaknessesJson,
            draft.RecommendationsJson,
            draft.MissingKeywordsJson,
            draft.StrengthsJson,
            timeProvider.GetUtcNow());

        dbContext.ResumeAnalyses.Add(analysis);
        quotaService.RecordUsage(auth.Value.UserId, UsageActionType.AtsAnalysis, draft.Usage);
        await dbContext.SaveChangesAsync(cancellationToken);

        loggerFactory.CreateLogger("AiUsage").LogInformation(
            "ATS analysis generated. UserId: {UserId}, ResumeId: {ResumeId}, TokensUsed: {TokensUsed}, ProcessingTimeMs: {ProcessingTimeMs}, EstimatedCost: {EstimatedCost}",
            auth.Value.UserId,
            resume.Id,
            draft.Usage.TokensUsed,
            draft.Usage.ProcessingTimeMs,
            draft.Usage.EstimatedCost);

        return TypedResults.Created($"/api/analyses/{analysis.Id}", analysis.ToResponse());
    }

    private static async Task<IResult> ListAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var analyses = await dbContext.ResumeAnalyses
            .Where(item => item.UserId == auth.Value!.UserId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(analyses.Select(item => item.ToResponse()).ToArray());
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var analysis = await dbContext.ResumeAnalyses.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == auth.Value!.UserId,
            cancellationToken);

        return analysis is null
            ? Result<AnalysisResponse>.Failure(Error.NotFound("analysis.not_found", "The analysis was not found.")).ToHttpResult()
            : TypedResults.Ok(analysis.ToResponse());
    }

    private static AnalysisResponse ToResponse(this ResumeAnalysis analysis) =>
        new(
            analysis.Id,
            analysis.ResumeId,
            analysis.AtsScore,
            analysis.KeywordMatchScore,
            analysis.SkillsCoverageScore,
            analysis.FormattingScore,
            analysis.ExperienceScore,
            analysis.EducationScore,
            ReadJsonArray(analysis.WeaknessesJson),
            ReadJsonArray(analysis.RecommendationsJson),
            ReadJsonArray(analysis.MissingKeywordsJson),
            ReadJsonArray(analysis.StrengthsJson),
            analysis.CreatedAt);

    private static IReadOnlyCollection<string> ReadJsonArray(string json) =>
        JsonSerializer.Deserialize<string[]>(json) ?? [];
}
