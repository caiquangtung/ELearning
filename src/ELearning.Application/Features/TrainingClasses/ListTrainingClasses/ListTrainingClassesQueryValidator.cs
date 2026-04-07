using FluentValidation;

namespace ELearning.Application.Features.TrainingClasses.ListTrainingClasses;

public sealed class ListTrainingClassesQueryValidator : AbstractValidator<ListTrainingClassesQuery>
{
    public ListTrainingClassesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
