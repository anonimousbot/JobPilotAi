using JobPilotAi_Backend.Core.Results;
using JobPilotAi_Backend.Infrastructure.Persistence;
using JobPilotAi_Backend.Modules.Billing;
using JobPilotAi_Backend.Modules.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace JobPilotAi_Backend.Core.Usage;

public sealed class QuotaService(ApplicationDbContext dbContext, TimeProvider timeProvider)
{
    public async Task<Result<int>> EnsureCanUseAsync(
        Guid userId,
        UsageActionType actionType,
        CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions
            .Where(item => item.UserId == userId && item.Status == SubscriptionStatus.Active)
            .OrderByDescending(item => item.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription?.PlanType == PlanType.Premium)
        {
            return Result<int>.Success(int.MaxValue);
        }

        var now = timeProvider.GetUtcNow();
        var periodStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddMonths(1);
        var used = await dbContext.UsageRecords
            .Where(usage => usage.UserId == userId
                && usage.ActionType == actionType
                && usage.PeriodStart == periodStart
                && usage.PeriodEnd == periodEnd)
            .SumAsync(usage => usage.Quantity, cancellationToken);
        const int freeLimit = 10;

        return used >= freeLimit
            ? Result<int>.Failure(Error.Forbidden("quota.exhausted", $"The monthly free quota for {actionType} has been used."))
            : Result<int>.Success(freeLimit - used);
    }

    public void RecordUsage(
        Guid userId,
        UsageActionType actionType,
        Ai.AiUsageMetrics metrics)
    {
        var now = timeProvider.GetUtcNow();
        var periodStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = periodStart.AddMonths(1);

        dbContext.UsageRecords.Add(UsageRecord.Create(
            userId,
            actionType,
            1,
            periodStart,
            periodEnd,
            now,
            metrics.TokensUsed,
            metrics.ProcessingTimeMs,
            metrics.EstimatedCost));
    }
}
