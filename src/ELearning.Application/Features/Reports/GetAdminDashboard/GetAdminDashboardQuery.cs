using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetAdminDashboard;

public sealed record GetAdminDashboardQuery : IRequest<Result<AdminDashboardDto>>;
