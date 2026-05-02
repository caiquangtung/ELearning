using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Commerce;

public sealed class OrderPaymentRepository(ApplicationDbContext context)
    : GenericRepository<OrderPayment>(context), IOrderPaymentRepository
{
    public Task<OrderPayment?> GetByExternalTransactionIdAsync(string externalTransactionId, CancellationToken ct = default) =>
        DbSet.FirstOrDefaultAsync(p => p.ExternalTransactionId == externalTransactionId, ct);
}
