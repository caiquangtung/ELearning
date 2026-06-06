using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class GetAiChatMessagesQueryHandler(
    IAiRagChatService chatService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAiChatMessagesQuery, Result<IReadOnlyList<AiChatMessageDto>>>
{
    public async Task<Result<IReadOnlyList<AiChatMessageDto>>> Handle(GetAiChatMessagesQuery request, CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue)
            return Result.Failure<IReadOnlyList<AiChatMessageDto>>(Error.Unauthorized());

        try
        {
            var messages = await chatService.GetMessagesAsync(currentUserService.UserId.Value, request.SessionId, ct);
            return messages.Select(AiChatMapper.ToDto).ToList();
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<IReadOnlyList<AiChatMessageDto>>(Error.NotFound("AiChatSession", request.SessionId));
        }
    }
}
