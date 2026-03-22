using System.Security.Claims;
using ELearning.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace ELearning.Infrastructure.Identity;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public IEnumerable<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? [];

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
