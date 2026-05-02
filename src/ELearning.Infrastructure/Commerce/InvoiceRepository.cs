using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Commerce;

public sealed class InvoiceRepository(ApplicationDbContext context)
    : GenericRepository<Invoice>(context), IInvoiceRepository
{
    public Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default) =>
        DbSet.FirstOrDefaultAsync(i => i.OrderId == orderId, ct);
}
