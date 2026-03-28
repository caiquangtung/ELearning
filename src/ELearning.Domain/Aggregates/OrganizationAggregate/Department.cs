using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.OrganizationAggregate;

public sealed class Department : Entity
{
    private Department() { }

    internal Department(Guid organizationId, string name, Guid? parentDepartmentId)
    {
        Id = Guid.NewGuid();
        OrganizationId = organizationId;
        Name = name;
        ParentDepartmentId = parentDepartmentId;
    }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public Guid? ParentDepartmentId { get; private set; }

    internal void Rename(string name) => Name = name;
}
