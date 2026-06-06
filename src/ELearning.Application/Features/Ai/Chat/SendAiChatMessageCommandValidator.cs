using FluentValidation;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class SendAiChatMessageCommandValidator : AbstractValidator<SendAiChatMessageCommand>
{
    public SendAiChatMessageCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(1000);
    }
}
