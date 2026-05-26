using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Users.AssignRoles;

public class AssignRolesToUserCommandHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogs)
    : IRequestHandler<AssignRolesToUserCommand, Result>
{
    public async Task<Result> Handle(AssignRolesToUserCommand request, CancellationToken ct)
    {
        if (!currentUser.Roles.Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase))
            return Result.Failure(Error.Forbidden("Only platform administrators can assign roles."));

        var user = await userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure(Error.NotFound("User", request.UserId));

        try
        {
            user.SetPlatformRoles(request.Roles);
            await unitOfWork.SaveChangesAsync(ct);
            await auditLogs.WriteAsync(new AuditLogEntry(
                "User.AssignRoles",
                "User",
                user.Id.ToString(),
                "Success",
                new Dictionary<string, string> { ["roles"] = string.Join(',', request.Roles) }), ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error("Domain.InvalidOperation", ex.Message));
        }
    }
}
