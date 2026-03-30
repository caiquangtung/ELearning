using ELearning.Domain.Aggregates.OrganizationAggregate;
using FluentValidation;

namespace ELearning.Application.Features.Organizations.AddMember;

public class AddMemberToOrganizationCommandValidator : AbstractValidator<AddMemberToOrganizationCommand>
{
    public AddMemberToOrganizationCommandValidator()
    {
        RuleFor(x => x.OrgRole)
            .NotEmpty()
            .Must(r => OrganizationRoles.IsValid(r))
            .WithMessage("Invalid organization role.");
    }
}
