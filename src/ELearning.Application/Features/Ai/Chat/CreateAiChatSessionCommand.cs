using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed record CreateAiChatSessionCommand(Guid? CourseId, string? Title)
    : IRequest<Result<AiChatSessionDto>>;
