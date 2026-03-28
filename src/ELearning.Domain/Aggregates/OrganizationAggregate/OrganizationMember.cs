using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.OrganizationAggregate;

public sealed class OrganizationMember : Entity
{
    private OrganizationMember() { }

    internal OrganizationMember(Guid organizationId, Guid userId, Guid? departmentId, string orgRole)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        UserId = userId;
        DepartmentId = departmentId;
        OrgRole = orgRole;
        JoinedAt = DateTime.UtcNow;
    }

    public Guid OrganizationId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public string OrgRole { get; private set; } = default!;
    public DateTime JoinedAt { get; private set; }

    internal void AssignDepartment(Guid? departmentId) => DepartmentId = departmentId;

    internal void SetOrgRole(string orgRole) => OrgRole = orgRole;
}
