using FluentValidation;

namespace ELearning.Application.Features.Licenses.ListLicensePools;

public sealed class ListLicensePoolsQueryValidator : AbstractValidator<ListLicensePoolsQuery>
{
    public ListLicensePoolsQueryValidator()
    {
        RuleFor(x => x.OrganizationId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(200);
    }
}
