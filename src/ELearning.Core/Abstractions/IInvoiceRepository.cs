using ELearning.Domain.Aggregates.CommerceAggregate;

namespace ELearning.Core.Abstractions;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);
}
