using ELearning.Core.Abstractions;
using ELearning.Core.Constants;

namespace ELearning.Seeder;

internal sealed class SystemCurrentUserService : ICurrentUserService
{
    public Guid? UserId => null;
    public string? Email => "seed@system.local";
    public IEnumerable<string> Roles => [ELearning.Core.Constants.Roles.Admin];
    public bool IsAuthenticated => true;

    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
}
