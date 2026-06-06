using System.Security.Cryptography;
using System.Text;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.AiAggregate;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class SendAiChatMessageCommandHandler(
    IAiRagChatService chatService,
    IAiRequestLogRepository aiRequestLogRepository,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SendAiChatMessageCommand, Result<AiChatAnswerDto>>
{
    private const string Feature = "RagLearningAssistant";
    private const string FailureProvider = "AI";
    private const string FailureModel = "unknown";
    private const string PromptVersion = "rag-learning-assistant-v1";

    public async Task<Result<AiChatAnswerDto>> Handle(SendAiChatMessageCommand request, CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue)
            return Result.Failure<AiChatAnswerDto>(Error.Unauthorized());

        var inputHash = ComputeInputHash(currentUserService.UserId.Value, request.SessionId, request.Message);
        try
        {
            var answer = await chatService.SendMessageAsync(
                currentUserService.UserId.Value,
                request.SessionId,
                request.Message,
                ct);

            aiRequestLogRepository.Add(AiRequestLog.Succeeded(
                currentUserService.UserId,
                Feature,
                answer.Provider,
                answer.Model,
                answer.PromptVersion,
                inputHash,
                answer.TokenEstimate));
            await unitOfWork.SaveChangesAsync(ct);

            return AiChatMapper.ToDto(answer);
        }
        catch (KeyNotFoundException)
        {
            return Result.Failure<AiChatAnswerDto>(Error.NotFound("AiChatSession", request.SessionId));
        }
        catch (Exception ex)
        {
            aiRequestLogRepository.Add(AiRequestLog.Failed(
                currentUserService.UserId,
                Feature,
                FailureProvider,
                FailureModel,
                PromptVersion,
                inputHash,
                ex.Message));
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<AiChatAnswerDto>(Error.Validation("AI.Chat", ex.Message));
        }
    }

    private static string ComputeInputHash(Guid userId, Guid sessionId, string message)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{userId}|{sessionId}|{message}|{PromptVersion}"));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
