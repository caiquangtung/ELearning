using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;
using System.Reflection;

namespace ELearning.Domain.UnitTests;

public class LicensePoolAggregateTests
{
    [Fact]
    public void Create_sets_properties()
    {
        var orgId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(10);

        var pool = LicensePool.Create(orgId, "Org pool", 5, expiresAt);

        pool.OrganizationId.Should().Be(orgId);
        pool.Name.Should().Be("Org pool");
        pool.TotalSeats.Should().Be(5);
        pool.ExpiresAt.Should().Be(expiresAt);
        pool.AvailableSeats.Should().Be(5);
        pool.ActiveSeatCount.Should().Be(0);
    }

    [Fact]
    public void Create_with_past_or_today_expiry_throws()
    {
        var act = () => LicensePool.Create(Guid.NewGuid(), "Pool", 5, DateTime.UtcNow.Date);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignSeat_decreases_available_seats()
    {
        var pool = LicensePool.Create(Guid.NewGuid(), "Pool", 2, null);

        pool.AssignSeat(Guid.NewGuid());

        pool.ActiveSeatCount.Should().Be(1);
        pool.AvailableSeats.Should().Be(1);
        pool.Assignments.Should().HaveCount(1);
        pool.Assignments.Single().IsActive.Should().BeTrue();
    }

    [Fact]
    public void AssignSeat_when_no_available_seats_throws()
    {
        var pool = LicensePool.Create(Guid.NewGuid(), "Pool", 1, null);
        pool.AssignSeat(Guid.NewGuid());

        var act = () => pool.AssignSeat(Guid.NewGuid());
        act.Should().Throw<DomainException>()
            .WithMessage("*No available seats*");
    }

    [Fact]
    public void AssignSeat_duplicate_active_assignment_throws()
    {
        var pool = LicensePool.Create(Guid.NewGuid(), "Pool", 2, null);
        var userId = Guid.NewGuid();

        pool.AssignSeat(userId);

        var act = () => pool.AssignSeat(userId);
        act.Should().Throw<DomainException>()
            .WithMessage("*already has an active license*");
    }

    [Fact]
    public void RevokeSeat_makes_assignment_inactive_and_frees_seat()
    {
        var pool = LicensePool.Create(Guid.NewGuid(), "Pool", 1, null);
        var userId = Guid.NewGuid();
        pool.AssignSeat(userId);

        pool.RevokeSeat(userId);

        pool.ActiveSeatCount.Should().Be(0);
        pool.AvailableSeats.Should().Be(1);
        pool.Assignments.Should().ContainSingle(a => a.UserId == userId && !a.IsActive && a.RevokedAt != null);
    }

    [Fact]
    public void AssignSeat_after_revoke_is_allowed()
    {
        var pool = LicensePool.Create(Guid.NewGuid(), "Pool", 1, null);
        var userId = Guid.NewGuid();
        pool.AssignSeat(userId);
        pool.RevokeSeat(userId);

        pool.AssignSeat(userId);

        pool.ActiveSeatCount.Should().Be(1);
        pool.Assignments.Count(a => a.UserId == userId).Should().Be(2);
    }

    [Fact]
    public void AssignSeat_when_expired_throws()
    {
        var pool = LicensePool.Create(Guid.NewGuid(), "Pool", 1, DateTime.UtcNow.AddDays(1));

        // Force expiry without waiting for time to pass
        typeof(LicensePool)
            .GetProperty(nameof(LicensePool.ExpiresAt), BindingFlags.Instance | BindingFlags.Public)!
            .SetValue(pool, DateTime.UtcNow.AddSeconds(-1));

        var act = () => pool.AssignSeat(Guid.NewGuid());
        act.Should().Throw<DomainException>()
            .WithMessage("*expired*");
    }
}

