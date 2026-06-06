using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed record SendAiChatMessageCommand(Guid SessionId, string Message)
    : IRequest<Result<AiChatAnswerDto>>;
