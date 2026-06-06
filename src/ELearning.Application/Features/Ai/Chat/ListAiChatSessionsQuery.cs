using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed record ListAiChatSessionsQuery : IRequest<Result<IReadOnlyList<AiChatSessionDto>>>;
