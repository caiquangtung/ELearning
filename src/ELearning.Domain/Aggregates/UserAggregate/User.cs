using ELearning.Domain.Events;
using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.UserAggregate;

public sealed class User : AggregateRoot
{
    private readonly List<string> _roles = [];

    private User() { }

    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public UserStatus Status { get; private set; }
    public IReadOnlyList<string> Roles => _roles.AsReadOnly();
    public string? RefreshTokenHash { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public static User Create(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        user._roles.Add(role);
        user.RaiseDomainEvent(new UserRegistered(user.Id, user.Email, user.FullName));
        return user;
    }

    public void AssignRole(string role)
    {
        if (_roles.Contains(role, StringComparer.OrdinalIgnoreCase)) return;
        _roles.Add(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveRole(string role)
    {
        if (_roles.Count == 1)
            throw new DomainException("User must have at least one role.");
        _roles.Remove(role);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetRefreshToken(string tokenHash, DateTime expiresAt)
    {
        RefreshTokenHash = tokenHash;
        RefreshTokenExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RevokeRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsRefreshTokenValid(string tokenHash) =>
        RefreshTokenHash == tokenHash &&
        RefreshTokenExpiresAt.HasValue &&
        RefreshTokenExpiresAt.Value > DateTime.UtcNow;

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        RevokeRefreshToken();
        RaiseDomainEvent(new PasswordChanged(Id, Email));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == UserStatus.Suspended) return;
        Status = UserStatus.Suspended;
        RevokeRefreshToken();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasRole(string role) =>
        _roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
