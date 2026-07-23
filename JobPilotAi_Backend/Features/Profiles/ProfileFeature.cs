using FluentValidation;
using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Core.Validation;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Profiles;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.Profiles;

public static class ProfileFeature
{
    public static IEndpointRouteBuilder MapProfileFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").WithTags("Profile");

        group.MapGet("", GetProfileAsync)
            .WithName("GetProfile")
            .WithSummary("Returns the authenticated user's profile.");

        group.MapPut("", UpdateProfileAsync)
            .WithName("UpdateProfile")
            .WithSummary("Updates the authenticated user's profile.");

        group.MapPut("/onboarding", UpdateOnboardingAsync)
            .WithName("UpdateOnboardingProfile")
            .WithSummary("Saves the career goal, target role, and experience level collected during onboarding.");

        return app;
    }

    private static async Task<IResult> GetProfileAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var profile = await GetOrCreateProfileAsync(dbContext, auth.Value!.UserId, timeProvider.GetUtcNow(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(profile.ToResponse());
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateProfileRequest request,
        IValidator<UpdateProfileRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<ProfileResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var now = timeProvider.GetUtcNow();
        var profile = await GetOrCreateProfileAsync(dbContext, auth.Value!.UserId, now, cancellationToken);
        profile.Update(
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Location,
            request.LinkedInUrl,
            request.PortfolioUrl,
            request.CareerGoal,
            request.TargetRole,
            request.CareerLevel,
            now);

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(profile.ToResponse());
    }

    private static async Task<IResult> UpdateOnboardingAsync(
        UpdateOnboardingProfileRequest request,
        IValidator<UpdateOnboardingProfileRequest> validator,
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result<ProfileResponse>.Failure(Error.Validation(validationResult.ToErrorDictionary())).ToHttpResult();
        }

        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var now = timeProvider.GetUtcNow();
        var profile = await GetOrCreateProfileAsync(dbContext, auth.Value!.UserId, now, cancellationToken);
        profile.UpdateOnboarding(request.CareerGoal, request.TargetRole, request.CareerLevel, request.CurrentJobTitle, request.YearsOfExperience, now);

        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(profile.ToResponse());
    }

    private static async Task<Profile> GetOrCreateProfileAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.Profiles.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        if (profile is not null)
        {
            return profile;
        }

        profile = Profile.Create(userId, now);
        dbContext.Profiles.Add(profile);
        return profile;
    }

    private static ProfileResponse ToResponse(this Profile profile) =>
        new(
            profile.Id,
            profile.UserId,
            profile.FirstName,
            profile.LastName,
            profile.PhoneNumber,
            profile.Location,
            profile.LinkedInUrl,
            profile.PortfolioUrl,
            profile.CareerGoal,
            profile.TargetRole,
            profile.CareerLevel,
            profile.UpdatedAt);
}
