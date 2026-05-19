using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetInstructorDashboard;

public sealed record GetInstructorDashboardQuery : IRequest<Result<InstructorDashboardDto>>;
