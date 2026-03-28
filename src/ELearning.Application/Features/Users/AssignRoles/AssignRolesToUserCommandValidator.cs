using ELearning.Core.Constants;
using FluentValidation;

namespace ELearning.Application.Features.Users.AssignRoles;

public class AssignRolesToUserCommandValidator : AbstractValidator<AssignRolesToUserCommand>
{
    public AssignRolesToUserCommandValidator()
    {
        RuleFor(x => x.Roles)
            .NotEmpty()
            .Must(roles => roles.All(r => Roles.IsValid(r)))
            .WithMessage("One or more roles are invalid.");
    }
}
