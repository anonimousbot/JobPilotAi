using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Core.Security;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Features.Subscriptions;

public static class SubscriptionFeature
{
    public static IEndpointRouteBuilder MapSubscriptionFeature(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions").WithTags("Subscriptions");

        group.MapGet("/current", GetCurrentAsync).WithName("GetCurrentSubscription");
        group.MapPost("/upgrade", UpgradeAsync).WithName("UpgradeSubscription");
        group.MapPost("/cancel", CancelAsync).WithName("CancelSubscription");
        group.MapGet("/history", HistoryAsync).WithName("GetSubscriptionHistory");

        return app;
    }

    private static async Task<IResult> GetCurrentAsync(
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

        var subscription = await GetOrCreateCurrentAsync(dbContext, auth.Value!.UserId, timeProvider.GetUtcNow(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(subscription.ToResponse());
    }

    private static async Task<IResult> UpgradeAsync(
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

        var subscription = await GetOrCreateCurrentAsync(dbContext, auth.Value!.UserId, timeProvider.GetUtcNow(), cancellationToken);
        subscription.Upgrade(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(subscription.ToResponse());
    }

    private static async Task<IResult> CancelAsync(
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

        var subscription = await GetOrCreateCurrentAsync(dbContext, auth.Value!.UserId, timeProvider.GetUtcNow(), cancellationToken);
        subscription.Cancel(timeProvider.GetUtcNow());
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(subscription.ToResponse());
    }

    private static async Task<IResult> HistoryAsync(
        CurrentUserAccessor currentUser,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var auth = currentUser.GetRequiredUser();
        if (!auth.IsSuccess)
        {
            return auth.ToHttpResult();
        }

        var history = await dbContext.Subscriptions
            .Where(item => item.UserId == auth.Value!.UserId)
            .OrderByDescending(item => item.StartDate)
            .ToListAsync(cancellationToken);

        return TypedResults.Ok(history.Select(item => item.ToResponse()).ToArray());
    }

    private static async Task<Subscription> GetOrCreateCurrentAsync(
        ApplicationDbContext dbContext,
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions
            .Where(item => item.UserId == userId && item.Status == SubscriptionStatus.Active)
            .OrderByDescending(item => item.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription is not null)
        {
            return subscription;
        }

        subscription = Subscription.CreateFree(userId, now);
        dbContext.Subscriptions.Add(subscription);
        return subscription;
    }

    private static SubscriptionResponse ToResponse(this Subscription subscription) =>
        new(
            subscription.Id,
            subscription.PlanType.ToString(),
            subscription.Status.ToString(),
            subscription.StartDate,
            subscription.EndDate,
            subscription.BillingCycle);
}
