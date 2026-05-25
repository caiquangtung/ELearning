using FluentValidation;

namespace ELearning.Application.Features.Promotions.Campaigns.ListCampaigns;

public sealed class ListCampaignsQueryValidator : AbstractValidator<ListCampaignsQuery>
{
    public ListCampaignsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(200);
    }
}
