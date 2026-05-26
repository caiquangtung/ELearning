using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.AddRule;

public sealed class AddItemPercentOffRuleCommandHandler(
    ICampaignRepository campaigns,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogs)
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
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Campaign.AddRule",
                "Campaign",
                campaign.Id.ToString(),
                "Success",
                new Dictionary<string, string>
                {
                    ["percentOff"] = request.PercentOff.ToString("0.##"),
                    ["itemTypes"] = string.Join(',', request.AppliesToItemTypes)
                }), ct);

            return CampaignDtoMapper.ToDto(campaign);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CampaignDto>(Error.Conflict("Campaign", ex.Message));
        }
    }
}
