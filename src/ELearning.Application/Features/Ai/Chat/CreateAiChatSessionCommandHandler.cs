using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class CreateAiChatSessionCommandHandler(
    IAiRagChatService chatService,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateAiChatSessionCommand, Result<AiChatSessionDto>>
{
    public async Task<Result<AiChatSessionDto>> Handle(CreateAiChatSessionCommand request, CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue)
            return Result.Failure<AiChatSessionDto>(Error.Unauthorized());

        try
        {
            var session = await chatService.CreateSessionAsync(
                currentUserService.UserId.Value,
                currentUserService.Roles.ToArray(),
                request.CourseId,
                request.Title,
                ct);
            return AiChatMapper.ToDto(session);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<AiChatSessionDto>(Error.NotFound("Course", request.CourseId ?? Guid.Empty));
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure<AiChatSessionDto>(Error.Validation("AI.Chat", ex.Message));
        }
    }
}
