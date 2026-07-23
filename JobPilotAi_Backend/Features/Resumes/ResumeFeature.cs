using FluentValidation;
using JobPilotAi_Backend.Core.Files;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Resumes;
using JobPilotAi_Backend.Modules.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.Resumes;

public static class ResumeFeature
{
    public static IEndpointRouteBuilder MapResumeFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/resumes").WithTags("Resumes");

        group.MapPost("", UploadAsync)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("UploadResume")
            .WithSummary("Uploads a resume file for the authenticated user.");

        group.MapGet("", ListAsync)
            .WithName("ListResumes")
            .WithSummary("Lists resumes owned by the authenticated user.");

        group.MapGet("/{id:guid}", GetAsync)
            .WithName("GetResume")
            .WithSummary("Gets a resume owned by the authenticated user.");

        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteResume")
            .WithSummary("Deletes a resume owned by the authenticated user.");

        return app;
    }

    private static async Task<IResult> UploadAsync(
        IFormFile file,
        IValidator<UploadResumeRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        IResumeStorage storage,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var request = new UploadResumeRequest(file);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<UploadResumeResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var userId = auth.Value!.UserId;
        var subscription = await dbContext.Subscriptions
            .Where(item => item.UserId == userId && item.Status == SubscriptionStatus.Active)
            .OrderByDescending(item => item.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        var storedResumeCount = await dbContext.Resumes.CountAsync(item => item.UserId == userId, cancellationToken);
        if (subscription?.PlanType != PlanType.Premium && storedResumeCount >= 5)
        {
            return Result<UploadResumeResponse>
                .Failure(Error.Forbidden("resume.free_limit_reached", "Free users can store up to 5 resumes."))
                .ToHttpResult();
        }

        var stored = await storage.SaveAsync(userId, file, cancellationToken);
        var duplicateFound = await dbContext.Resumes
            .AnyAsync(item => item.UserId == userId && item.ContentHash == stored.ContentHash, cancellationToken);
        var resume = Resume.Create(
            userId,
            file.FileName,
            Path.GetExtension(file.FileName).ToLowerInvariant(),
            file.Length,
            stored.StoragePath,
            stored.ContentHash,
            stored.ExtractedText,
            timeProvider.GetUtcNow());

        dbContext.Resumes.Add(resume);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created(
            $"/api/resumes/{resume.Id}",
            new UploadResumeResponse(resume.Id, resume.FileName, resume.FileType, resume.FileSize, duplicateFound, resume.UploadedAt));
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

        var resumes = await dbContext.Resumes
            .Where(resume => resume.UserId == auth.Value!.UserId)
            .OrderByDescending(resume => resume.UploadedAt)
            .Select(resume => resume.ToResponse())
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(resumes);
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

        var resume = await dbContext.Resumes
            .Where(item => item.Id == id && item.UserId == auth.Value!.UserId)
            .Select(item => item.ToResponse())
            .SingleOrDefaultAsync(cancellationToken);

        return resume is null
            ? Result<ResumeResponse>.Failure(Error.NotFound("resume.not_found", "The resume was not found.")).ToHttpResult()
            : TypedResults.Ok(resume);
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

        var resume = await dbContext.Resumes.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == auth.Value!.UserId,
            cancellationToken);

        if (resume is null)
        {
            return Result<object>.Failure(Error.NotFound("resume.not_found", "The resume was not found.")).ToHttpResult();
        }

        dbContext.Resumes.Remove(resume);
        await dbContext.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static ResumeResponse ToResponse(this Resume resume) =>
        new(resume.Id, resume.FileName, resume.FileType, resume.FileSize, resume.ContentHash, resume.UploadedAt);
}
