using ELearning.Application.Features.Reports.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Reports.GetStudentDashboard;

public sealed record GetStudentDashboardQuery : IRequest<Result<StudentDashboardDto>>;
