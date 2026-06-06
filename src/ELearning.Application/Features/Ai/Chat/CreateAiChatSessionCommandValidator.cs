using FluentValidation;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class CreateAiChatSessionCommandValidator : AbstractValidator<CreateAiChatSessionCommand>
{
    public CreateAiChatSessionCommandValidator()
    {
        RuleFor(x => x.Title).MaximumLength(200);
    }
}
