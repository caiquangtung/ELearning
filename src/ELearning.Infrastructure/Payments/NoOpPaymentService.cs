using ELearning.Application.Common.Interfaces;

namespace ELearning.Infrastructure.Payments;

/// <summary>Development provider: verification succeeds for any non-empty transaction id.</summary>
public sealed class NoOpPaymentService : IPaymentService
{
    public Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default)
    {
        var txn = $"noop-{request.OrderId:N}-{Guid.NewGuid():N}";
        return Task.FromResult(new PaymentResult(txn, "created", PaymentUrl: null));
    }

    public Task<bool> VerifyPaymentAsync(string transactionId, CancellationToken ct = default) =>
        Task.FromResult(!string.IsNullOrWhiteSpace(transactionId));

    public Task<bool> RefundAsync(string transactionId, decimal amount, CancellationToken ct = default) =>
        Task.FromResult(true);
}
