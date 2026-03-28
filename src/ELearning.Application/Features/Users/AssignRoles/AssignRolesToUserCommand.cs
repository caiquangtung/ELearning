using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Users.AssignRoles;

public record AssignRolesToUserCommand(Guid UserId, IReadOnlyList<string> Roles)
    : IRequest<Result>;
