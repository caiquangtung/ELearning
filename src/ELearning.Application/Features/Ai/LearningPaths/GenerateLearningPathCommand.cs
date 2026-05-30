using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.LearningPaths;

public sealed record GenerateLearningPathCommand(
    string Goal,
    string? CurrentSkills,
    string? TargetRole,
    Guid? OrganizationId,
    int MaxCourses) : IRequest<Result<LearningPathDraftDto>>;
