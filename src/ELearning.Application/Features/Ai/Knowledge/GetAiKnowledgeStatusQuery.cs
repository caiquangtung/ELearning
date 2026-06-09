using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Knowledge;

public sealed record GetAiKnowledgeStatusQuery : IRequest<Result<AiKnowledgeStatusDto>>;
