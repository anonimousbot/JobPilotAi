using FluentValidation;
using JobPilotAi_Backend.Core.Ai;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Usage;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Billing;
using JobPilotAi_Backend.Modules.CoverLetters;
using JobPilotAi_Backend.Modules.Resumes;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.CoverLetters;

public static class CoverLetterFeature
{
    public static IEndpointRouteBuilder MapCoverLetterFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/coverletters").WithTags("Cover Letters");

        group.MapPost("", CreateAsync)
            .WithName("CreateCoverLetter")
            .WithSummary("Generates a tailored cover letter.");

        group.MapPost("/{id:guid}/regenerate", RegenerateAsync)
            .WithName("RegenerateCoverLetter")
            .WithSummary("Regenerates an existing cover letter and consumes quota.");

        group.MapGet("", ListAsync)
            .WithName("ListCoverLetters")
            .WithSummary("Lists cover letters for the authenticated user.");

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetCoverLetter")
            .WithSummary("Gets one cover letter for the authenticated user.");

        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateCoverLetter")
            .WithSummary("Updates the edited cover letter content.");

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteCoverLetter")
            .WithSummary("Deletes a cover letter.");

        return app;
    }

    private static async Task<IResult> CreateAsync(
        CreateCoverLetterRequest request,
        IValidator<CreateCoverLetterRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        IAiProvider aiProvider,
        QuotaService quotaService,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CoverLetterResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var user = await dbContext.Users.FindAsync([auth.Value!.UserId], cancellationToken);
        if (user?.EmailConfirmed != true)
        {
            return Result<CoverLetterResponse>.Failure(Error.Forbidden("auth.email_not_verified", "Email verification is required before cover letter generation.")).ToHttpResult();
        }

        var resume = await GetResumeAsync(dbContext, auth.Value.UserId, request.ResumeId, cancellationToken);
        if (request.ResumeId.HasValue && resume is null)
        {
            return Result<CoverLetterResponse>.Failure(Error.NotFound("resume.not_found", "The resume was not found.")).ToHttpResult();
        }

        var quota = await quotaService.EnsureCanUseAsync(auth.Value.UserId, UsageActionType.CoverLetterGeneration, cancellationToken);
        if (!quota.IsSuccess)
        {
            return quota.ToHttpResult();
        }

        var tone = NormalizeTone(request.Tone);
        CoverLetterDraft draft;
        try
        {
            draft = await aiProvider.GenerateCoverLetterAsync(
                resume,
                request.JobTitle,
                request.CompanyName,
                request.JobDescription,
                tone,
                cancellationToken);
        }
        catch (Exception exception)
        {
            loggerFactory.CreateLogger<OpenAiProvider>().LogWarning(
                exception,
                "Cover letter provider failed. UserId: {UserId}, ResumeId: {ResumeId}",
                auth.Value.UserId,
                request.ResumeId);

            return Result<CoverLetterResponse>
                .Failure(Error.Unavailable("ai.provider_unavailable", "The AI provider is currently unavailable."))
                .ToHttpResult();
        }

        var coverLetter = CoverLetter.Create(
            auth.Value.UserId,
            request.ResumeId,
            request.JobTitle,
            request.CompanyName,
            request.JobDescription,
            tone,
            draft.Content,
            timeProvider.GetUtcNow());

        dbContext.CoverLetters.Add(coverLetter);
        quotaService.RecordUsage(auth.Value.UserId, UsageActionType.CoverLetterGeneration, draft.Usage);
        await dbContext.SaveChangesAsync(cancellationToken);

        loggerFactory.CreateLogger("AiUsage").LogInformation(
            "Cover letter generated. UserId: {UserId}, CoverLetterId: {CoverLetterId}, TokensUsed: {TokensUsed}, ProcessingTimeMs: {ProcessingTimeMs}, EstimatedCost: {EstimatedCost}",
            auth.Value.UserId,
            coverLetter.Id,
            draft.Usage.TokensUsed,
            draft.Usage.ProcessingTimeMs,
            draft.Usage.EstimatedCost);

        return TypedResults.Created($"/api/coverletters/{coverLetter.Id}", coverLetter.ToResponse());
    }

    private static async Task<IResult> RegenerateAsync(
        Guid id,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        IAiProvider aiProvider,
        QuotaService quotaService,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var userId = auth.Value!.UserId;
        var coverLetter = await dbContext.CoverLetters.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == userId,
            cancellationToken);
        if (coverLetter is null)
        {
            return Result<CoverLetterResponse>.Failure(Error.NotFound("coverletter.not_found", "The cover letter was not found.")).ToHttpResult();
        }

        var quota = await quotaService.EnsureCanUseAsync(userId, UsageActionType.CoverLetterGeneration, cancellationToken);
        if (!quota.IsSuccess)
        {
            return quota.ToHttpResult();
        }

        var resume = await GetResumeAsync(dbContext, userId, coverLetter.ResumeId, cancellationToken);
        CoverLetterDraft draft;
        try
        {
            draft = await aiProvider.GenerateCoverLetterAsync(
                resume,
                coverLetter.JobTitle,
                coverLetter.CompanyName,
                coverLetter.JobDescription,
                coverLetter.Tone,
                cancellationToken);
        }
        catch
        {
            return Result<CoverLetterResponse>
                .Failure(Error.Unavailable("ai.provider_unavailable", "The AI provider is currently unavailable."))
                .ToHttpResult();
        }

        coverLetter.Regenerate(draft.Content, timeProvider.GetUtcNow());
        quotaService.RecordUsage(userId, UsageActionType.CoverLetterGeneration, draft.Usage);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(coverLetter.ToResponse());
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

        var letters = await dbContext.CoverLetters
            .Where(item => item.UserId == auth.Value!.UserId)
            .OrderByDescending(item => item.CreatedAt)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(letters.Select(item => item.ToResponse()).ToArray());
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

        var letter = await dbContext.CoverLetters.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == auth.Value!.UserId,
            cancellationToken);

        return letter is null
            ? Result<CoverLetterResponse>.Failure(Error.NotFound("coverletter.not_found", "The cover letter was not found.")).ToHttpResult()
            : TypedResults.Ok(letter.ToResponse());
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateCoverLetterRequest request,
        IValidator<UpdateCoverLetterRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<CoverLetterResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var letter = await dbContext.CoverLetters.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == auth.Value!.UserId,
            cancellationToken);
        if (letter is null)
        {
            return Result<CoverLetterResponse>.Failure(Error.NotFound("coverletter.not_found", "The cover letter was not found.")).ToHttpResult();
        }

        letter.Edit(request.EditedContent, timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(letter.ToResponse());
    }

    private static async Task<IResult> DeleteAsync(
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

        var letter = await dbContext.CoverLetters.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == auth.Value!.UserId,
            cancellationToken);
        if (letter is null)
        {
            return Result<CoverLetterResponse>.Failure(Error.NotFound("coverletter.not_found", "The cover letter was not found.")).ToHttpResult();
        }

        dbContext.CoverLetters.Remove(letter);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.NoContent();
    }

    private static Task<Resume?> GetResumeAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        Guid? resumeId,
        CancellationToken cancellationToken) =>
        resumeId is null
            ? Task.FromResult<Resume?>(null)
            : dbContext.Resumes.SingleOrDefaultAsync(item => item.Id == resumeId && item.UserId == userId, cancellationToken);

    private static string NormalizeTone(string? tone) =>
        string.IsNullOrWhiteSpace(tone)
            ? "Professional"
            : string.Concat(tone[..1].ToUpperInvariant(), tone[1..].ToLowerInvariant());

    private static CoverLetterResponse ToResponse(this CoverLetter letter) =>
        new(
            letter.Id,
            letter.ResumeId,
            letter.JobTitle,
            letter.CompanyName,
            letter.JobDescription,
            letter.Tone,
            letter.OriginalContent,
            letter.EditedContent,
            letter.CreatedAt,
            letter.UpdatedAt);
}
