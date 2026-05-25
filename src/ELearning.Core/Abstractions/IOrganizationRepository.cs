using ELearning.Core.Common;
using ELearning.Domain.Aggregates.OrganizationAggregate;

namespace ELearning.Core.Abstractions;

public interface IOrganizationRepository : IRepository<Organization>
{
    Task<Organization?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Organization?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct = default);
    Task<OrganizationMember?> FindMembershipAsync(Guid organizationId, Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Organization>> GetOrganizationsForUserAsync(Guid userId, CancellationToken ct = default);
    Task<PagedList<Organization>> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task<PagedList<Organization>> ListForUserAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
}
