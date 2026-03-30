using System.Security.Claims;
using ELearning.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ELearning.WebApi.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        var perms = PermissionMap.GetPermissionsForRoles(roles);
        if (perms.Contains(requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
