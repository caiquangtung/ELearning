using Microsoft.AspNetCore.Authorization;

namespace ELearning.WebApi.Authorization;

/// <summary>
/// Requires a permission resolved via <see cref="PermissionPolicyProvider"/> and <see cref="PermissionAuthorizationHandler"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) => Policy = $"Permission:{permission}";
}
