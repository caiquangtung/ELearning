namespace ELearning.Application.Common.Options;

public sealed class PaymentOptions
{
    public const string SectionName = "Payments";

    /// <summary>NoOp completes payment immediately after intent creation (development).</summary>
    public string Provider { get; init; } = "NoOp";

    /// <summary>Shared secret for POST /payments/webhook (header X-Payments-Webhook-Secret).</summary>
    public string WebhookSecret { get; init; } = "";
}
