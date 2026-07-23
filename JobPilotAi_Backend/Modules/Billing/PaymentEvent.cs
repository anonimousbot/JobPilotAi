namespace JobPilotAi_Backend.Modules.Billing;

public sealed class PaymentEvent
{
    public Guid Id { get; private set; }

    public Guid SubscriptionId { get; private set; }

    public decimal Amount { get; private set; }

    public string Currency { get; private set; } = "USD";

    public string Provider { get; private set; } = string.Empty;

    public string ProviderReference { get; private set; } = string.Empty;

    public string Status { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }
}
