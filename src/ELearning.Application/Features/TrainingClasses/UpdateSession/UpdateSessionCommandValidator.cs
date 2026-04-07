using ELearning.Domain.Aggregates.TrainingClassAggregate;
using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.UpdateSession;

public sealed class UpdateSessionCommandValidator : AbstractValidator<UpdateSessionCommand>
{
    public UpdateSessionCommandValidator()
    {
        RuleFor(x => x.TrainingClassId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EndUtc).GreaterThan(x => x.StartUtc);
        RuleFor(x => x.Location).NotEmpty().When(x => x.SessionType == ClassSessionType.Offline);
        RuleFor(x => x.Location).MaximumLength(500).When(x => x.Location is not null);
    }
}
