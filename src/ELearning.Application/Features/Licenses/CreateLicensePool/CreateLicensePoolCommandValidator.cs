using FluentValidation;

namespace ELearning.Application.Features.Licenses.CreateLicensePool;

public sealed class CreateLicensePoolCommandValidator : AbstractValidator<CreateLicensePoolCommand>
{
    public CreateLicensePoolCommandValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalSeats).GreaterThan(0).LessThanOrEqualTo(100_000);
    }
}

