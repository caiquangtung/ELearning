using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.EssayGrading;

public sealed record SuggestEssayGradesCommand(Guid AttemptId, string? Rubric)
    : IRequest<Result<EssayGradeSuggestionsDto>>;
