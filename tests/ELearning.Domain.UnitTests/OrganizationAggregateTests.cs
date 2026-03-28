using ELearning.Domain.Aggregates.OrganizationAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class OrganizationAggregateTests
{
    [Fact]
    public void Create_sets_slug_lowercase()
    {
        var org = Organization.Create("Acme Corp", "acme-corp");
        org.Slug.Should().Be("acme-corp");
        org.Name.Should().Be("Acme Corp");
    }

    [Fact]
    public void AddMember_twice_throws()
    {
        var org = Organization.Create("Acme", "acme");
        var userId = Guid.NewGuid();
        org.AddMember(userId, null, OrganizationRoles.Member);

        var act = () => org.AddMember(userId, null, OrganizationRoles.Member);
        act.Should().Throw<DomainException>();
    }
}
