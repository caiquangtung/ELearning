using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.GetTrainingClass;

public sealed class GetTrainingClassQueryValidator : AbstractValidator<GetTrainingClassQuery>
{
    public GetTrainingClassQueryValidator() =>
        RuleFor(x => x.Id).NotEmpty();
}
