using ELearning.Application.Features.Promotions.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Promotions.Campaigns.GetCampaign;

public sealed record GetCampaignQuery(Guid Id) : IRequest<Result<CampaignDto>>;

