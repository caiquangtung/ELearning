using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.ListTrainingClasses;

public sealed record ListTrainingClassesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CourseId = null,
    string? Search = null)
    : IRequest<Result<PagedList<TrainingClassListItemDto>>>;
