using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.AddRule;

public sealed class AddItemPercentOffRuleCommandHandler(
    ICampaignRepository campaigns,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddItemPercentOffRuleCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(AddItemPercentOffRuleCommand request, CancellationToken ct)
    {
        try
        {
            var campaign = await campaigns.GetByIdWithRulesAndCouponsAsync(request.CampaignId, ct);
            if (campaign is null)
                return Result.Failure<CampaignDto>(Error.NotFound("Campaign", request.CampaignId));

            campaign.AddItemPercentOffRule(request.PercentOff, request.AppliesToItemTypes, DateTime.UtcNow);
            campaigns.Update(campaign);
            await unitOfWork.SaveChangesAsync(ct);

            return CampaignDtoMapper.ToDto(campaign);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CampaignDto>(Error.Conflict("Campaign", ex.Message));
        }
    }
}

