using Microsoft.AspNetCore.Authorization;

namespace ELearning.WebApi.Authorization;

/// <summary>
/// Marks an endpoint as requiring a specific permission.
/// The permission is resolved at runtime via PermissionPolicyProvider → PermissionAuthorizationHandler.
/// Usage: [HasPermission(Permissions.Courses.Create)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class HasPermissionAttribute(string permission) : AuthorizeAttribute(permission);
