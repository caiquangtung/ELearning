using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed record GetAccessibleAiCoursesQuery : IRequest<Result<IReadOnlyList<AiAccessibleCourseDto>>>;
