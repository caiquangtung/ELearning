using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class ListAiChatSessionsQueryHandler(
    IAiRagChatService chatService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListAiChatSessionsQuery, Result<IReadOnlyList<AiChatSessionDto>>>
{
    public async Task<Result<IReadOnlyList<AiChatSessionDto>>> Handle(ListAiChatSessionsQuery request, CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue)
            return Result.Failure<IReadOnlyList<AiChatSessionDto>>(Error.Unauthorized());

        var sessions = await chatService.ListSessionsAsync(currentUserService.UserId.Value, ct);
        return sessions.Select(AiChatMapper.ToDto).ToList();
    }
}
