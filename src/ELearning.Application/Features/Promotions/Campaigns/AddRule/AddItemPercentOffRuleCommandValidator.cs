using FluentValidation;

namespace ELearning.Application.Features.Promotions.Campaigns.AddRule;

public sealed class AddItemPercentOffRuleCommandValidator : AbstractValidator<AddItemPercentOffRuleCommand>
{
    public AddItemPercentOffRuleCommandValidator()
    {
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.PercentOff).InclusiveBetween(1, 100);
        RuleFor(x => x.AppliesToItemTypes).NotEmpty();
    }
}

