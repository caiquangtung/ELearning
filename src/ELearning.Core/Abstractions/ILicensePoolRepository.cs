using ELearning.Core.Common;
using ELearning.Domain.Aggregates.LicensePoolAggregate;

namespace ELearning.Core.Abstractions;

public interface ILicensePoolRepository : IRepository<LicensePool>
{
    Task<IReadOnlyList<LicensePool>> ListByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<LicensePool?> GetByIdWithAssignmentsAsync(Guid id, CancellationToken ct = default);
}

