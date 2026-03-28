using ELearning.Core.Abstractions;

namespace ELearning.Infrastructure.Identity;

/// <summary>Used at design-time (EF migrations) when no HTTP context exists.</summary>
public sealed class NullCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public string? Email => null;
    public IEnumerable<string> Roles => [];
    public bool IsAuthenticated => false;
    public bool HasRole(string role) => false;
}
