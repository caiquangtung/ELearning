using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.AddRule;

public sealed record AddItemPercentOffRuleCommand(
    Guid CampaignId,
    int PercentOff,
    IReadOnlyList<string> AppliesToItemTypes)
    : IRequest<Result<CampaignDto>>;

