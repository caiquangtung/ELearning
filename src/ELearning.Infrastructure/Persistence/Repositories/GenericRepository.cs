using ELearning.Core.Abstractions;
using ELearning.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ELearning.Infrastructure.Persistence.Repositories;

public class GenericRepository<T>(ApplicationDbContext context) : IRepository<T>
    where T : AggregateRoot
{
    protected readonly DbSet<T> DbSet = context.Set<T>();

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await DbSet.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await DbSet.ToListAsync(ct);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.Where(predicate).ToListAsync(ct);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await DbSet.AnyAsync(predicate, ct);

    public void Add(T entity) => DbSet.Add(entity);
    public void Update(T entity) => DbSet.Update(entity);
    public void Remove(T entity) => DbSet.Remove(entity);
}
