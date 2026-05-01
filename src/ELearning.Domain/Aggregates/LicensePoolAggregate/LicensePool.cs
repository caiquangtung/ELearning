using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.LicensePoolAggregate;

public sealed class LicensePool : AuditableAggregateRoot
{
    private LicensePool() { }

    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = default!;
    public int TotalSeats { get; private set; }

    /// <summary>Optional business constraint; null means no expiry.</summary>
    public DateTime? ExpiresAt { get; private set; }

    public List<LicenseAssignment> Assignments { get; private set; } = [];

    public static LicensePool Create(Guid organizationId, string name, int totalSeats, DateTime? expiresAt)
    {
        if (organizationId == Guid.Empty) throw new DomainException("OrganizationId is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (totalSeats <= 0) throw new DomainException("Total seats must be greater than 0.");

        if (expiresAt.HasValue && expiresAt.Value <= DateTime.UtcNow.Date)
            throw new DomainException("Expiry must be in the future.");

        return new LicensePool
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name.Trim(),
            TotalSeats = totalSeats,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };
    }

    public int ActiveSeatCount => Assignments.Count(a => a.IsActive);
    public int AvailableSeats => Math.Max(0, TotalSeats - ActiveSeatCount);

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeSeatCount(int totalSeats)
    {
        if (totalSeats <= 0) throw new DomainException("Total seats must be greater than 0.");
        if (totalSeats < ActiveSeatCount) throw new DomainException("Total seats cannot be less than assigned seats.");
        TotalSeats = totalSeats;
        UpdatedAt = DateTime.UtcNow;
    }

    public LicenseAssignment AssignSeat(Guid userId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");
        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow)
            throw new DomainException("License pool has expired.");

        if (Assignments.Any(a => a.UserId == userId && a.IsActive))
            throw new DomainException("User already has an active license in this pool.");

        if (AvailableSeats <= 0)
            throw new DomainException("No available seats in this pool.");

        var assignment = LicenseAssignment.Create(Id, OrganizationId, userId);
        Assignments.Add(assignment);
        UpdatedAt = DateTime.UtcNow;
        return assignment;
    }

    public void RevokeSeat(Guid userId)
    {
        if (userId == Guid.Empty) throw new DomainException("UserId is required.");

        var active = Assignments.FirstOrDefault(a => a.UserId == userId && a.IsActive)
            ?? throw new DomainException("User does not have an active license in this pool.");

        active.Revoke();
        UpdatedAt = DateTime.UtcNow;
    }
}

