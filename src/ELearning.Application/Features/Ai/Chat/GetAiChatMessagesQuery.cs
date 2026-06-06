using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed record GetAiChatMessagesQuery(Guid SessionId)
    : IRequest<Result<IReadOnlyList<AiChatMessageDto>>>;
