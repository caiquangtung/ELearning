using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.CreateCampaign;

public sealed class CreateCampaignCommandHandler(
    ICampaignRepository campaigns,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogs)
    : IRequestHandler<CreateCampaignCommand, Result<CampaignDto>>
{
    public async Task<Result<CampaignDto>> Handle(CreateCampaignCommand request, CancellationToken ct)
    {
        try
        {
            if (!Enum.TryParse<CampaignScope>(request.Scope.Trim(), ignoreCase: true, out var scope))
                return Result.Failure<CampaignDto>(Error.Validation("Scope", "Invalid campaign scope."));

            var campaign = Campaign.Create(
                request.Name,
                scope,
                request.OrganizationId,
                request.StartUtc,
                request.EndUtc);

            campaigns.Add(campaign);
            await unitOfWork.SaveChangesAsync(ct);
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Campaign.Create",
                "Campaign",
                campaign.Id.ToString(),
                "Success",
                new Dictionary<string, string> { ["scope"] = campaign.Scope.ToString() }), ct);

            return CampaignDtoMapper.ToDto(campaign);
        }
        catch (DomainException ex)
        {
            return Result.Failure<CampaignDto>(Error.Conflict("Campaign", ex.Message));
        }
    }
}
