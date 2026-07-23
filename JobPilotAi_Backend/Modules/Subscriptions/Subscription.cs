namespace JobPilotAi_Backend.Modules.Subscriptions;

public sealed class Subscription
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public PlanType PlanType { get; private set; } = PlanType.Free;

    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Active;

    public DateTimeOffset StartDate { get; private set; }

    public DateTimeOffset? EndDate { get; private set; }

    public string BillingCycle { get; private set; } = "Monthly";

    public static Subscription CreateFree(Guid userId, DateTimeOffset startedAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PlanType = PlanType.Free,
            Status = SubscriptionStatus.Active,
            StartDate = startedAt,
            BillingCycle = "Monthly"
        };

    public void Upgrade(DateTimeOffset upgradedAt)
    {
        PlanType = PlanType.Premium;
        Status = SubscriptionStatus.Active;
        StartDate = upgradedAt;
        EndDate = null;
    }

    public void Cancel(DateTimeOffset cancelledAt)
    {
        Status = SubscriptionStatus.Cancelled;
        EndDate = cancelledAt;
    }
}
