namespace ELearning.Application.Common.Interfaces;

public record PaymentRequest(Guid OrderId, decimal Amount, string Currency, string Description);
public record PaymentResult(string TransactionId, string Status, string? PaymentUrl);

public interface IPaymentService
{
    Task<PaymentResult> CreatePaymentAsync(PaymentRequest request, CancellationToken ct = default);
    Task<bool> VerifyPaymentAsync(string transactionId, CancellationToken ct = default);
    Task<bool> RefundAsync(string transactionId, decimal amount, CancellationToken ct = default);
}
