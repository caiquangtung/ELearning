using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.CourseRecommendations;

public sealed record GetCourseRecommendationsQuery(int Limit)
    : IRequest<Result<CourseRecommendationsDto>>;
