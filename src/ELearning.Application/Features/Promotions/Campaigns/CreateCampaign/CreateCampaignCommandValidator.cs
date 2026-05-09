using FluentValidation;

namespace ELearning.Application.Features.Promotions.Campaigns.CreateCampaign;

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).NotEmpty();
        RuleFor(x => x.StartUtc).NotEmpty();
    }
}

