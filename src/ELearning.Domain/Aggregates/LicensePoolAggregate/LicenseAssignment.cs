using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.LicensePoolAggregate;

public sealed class LicenseAssignment : Entity
{
    private LicenseAssignment() { }

    private LicenseAssignment(Guid licensePoolId, Guid organizationId, Guid userId)
    {
        Id = Guid.NewGuid();
        LicensePoolId = licensePoolId;
        OrganizationId = organizationId;
        UserId = userId;
        AssignedAt = DateTime.UtcNow;
    }

    public Guid LicensePoolId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }

    public DateTime AssignedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public bool IsActive => RevokedAt is null;

    internal static LicenseAssignment Create(Guid licensePoolId, Guid organizationId, Guid userId) =>
        new(licensePoolId, organizationId, userId);

    internal void Revoke()
    {
        if (RevokedAt is not null) return;
        RevokedAt = DateTime.UtcNow;
    }
}

