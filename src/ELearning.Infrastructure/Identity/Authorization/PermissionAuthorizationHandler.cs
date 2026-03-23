using System.Security.Claims;
using ELearning.Core.Constants;
using Microsoft.AspNetCore.Authorization;

namespace ELearning.Infrastructure.Identity.Authorization;

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Task.CompletedTask;

        var roles = context.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value);

        var permissions = PermissionMap.GetPermissionsForRoles(roles);

        if (permissions.Contains(requirement.Permission))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
