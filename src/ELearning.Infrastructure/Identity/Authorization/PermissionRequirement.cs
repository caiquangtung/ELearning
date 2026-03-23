using Microsoft.AspNetCore.Authorization;

namespace ELearning.Infrastructure.Identity.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
