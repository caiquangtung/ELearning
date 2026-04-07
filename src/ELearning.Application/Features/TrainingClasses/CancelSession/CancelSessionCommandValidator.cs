using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.CancelSession;

public sealed class CancelSessionCommandValidator : AbstractValidator<CancelSessionCommand>
{
    public CancelSessionCommandValidator()
    {
        RuleFor(x => x.TrainingClassId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
