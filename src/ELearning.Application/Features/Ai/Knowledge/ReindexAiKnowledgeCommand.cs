using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Knowledge;

public sealed record ReindexAiKnowledgeCommand(Guid? CourseId)
    : IRequest<Result<ReindexAiKnowledgeDto>>;
