using ELearning.Domain.Aggregates.CommerceAggregate;

namespace ELearning.Core.Abstractions;

public interface IOrderPaymentRepository : IRepository<OrderPayment>
{
    Task<OrderPayment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken ct = default);
}
