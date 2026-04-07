using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.OrganizationAggregate;

public sealed class Organization : AuditableAggregateRoot
{
    private Organization() { }

    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public OrganizationStatus Status { get; private set; }

    /// <summary>EF Core + domain: children owned by this aggregate.</summary>
    public List<Department> Departments { get; private set; } = [];

    public List<OrganizationMember> Members { get; private set; } = [];

    public static Organization Create(string name, string slug)
    {
        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Status = OrganizationStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
    }

    public Department AddDepartment(string name, Guid? parentDepartmentId)
    {
        if (parentDepartmentId.HasValue &&
            Departments.All(d => d.Id != parentDepartmentId.Value))
            throw new DomainException("Parent department does not belong to this organization.");

        var dept = new Department(Id, name.Trim(), parentDepartmentId);
        Departments.Add(dept);
        UpdatedAt = DateTime.UtcNow;
        return dept;
    }

    public OrganizationMember AddMember(Guid userId, Guid? departmentId, string orgRole)
    {
        if (!OrganizationRoles.IsValid(orgRole))
            throw new DomainException($"Invalid organization role: {orgRole}");

        if (Members.Any(m => m.UserId == userId))
            throw new DomainException("User is already a member of this organization.");

        if (departmentId.HasValue && Departments.All(d => d.Id != departmentId.Value))
            throw new DomainException("Department does not belong to this organization.");

        var member = new OrganizationMember(Id, userId, departmentId, orgRole);
        Members.Add(member);
        UpdatedAt = DateTime.UtcNow;
        return member;
    }

    public void RemoveMember(Guid userId)
    {
        var m = Members.FirstOrDefault(x => x.UserId == userId)
            ?? throw new DomainException("User is not a member of this organization.");
        Members.Remove(m);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rename(string name)
    {
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
