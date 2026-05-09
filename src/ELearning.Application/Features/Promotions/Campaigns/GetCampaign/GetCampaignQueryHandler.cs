using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.GetCampaign;

public sealed class GetCampaignQueryHandler(ICampaignRepository campaigns)
    : IRequestHandler<GetCampaignQuery, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(GetCampaignQuery request, CancellationToken ct)
    {
        var campaign = await campaigns.GetByIdWithRulesAndCouponsAsync(request.Id, ct);
        return campaign is null
            ? Result.Failure<CampaignDto>(Error.NotFound("Campaign", request.Id))
            : CampaignDtoMapper.ToDto(campaign);
    }
}

