using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.ListTrainingClasses;

public sealed class ListTrainingClassesQueryHandler(ITrainingClassRepository trainingClassRepository)
    : IRequestHandler<ListTrainingClassesQuery, Result<PagedList<TrainingClassListItemDto>>>
{
    public async Task<Result<PagedList<TrainingClassListItemDto>>> Handle(
        ListTrainingClassesQuery request,
        CancellationToken ct)
    {
        var page = await trainingClassRepository.ListAsync(
            request.Page,
            request.PageSize,
            request.CourseId,
            request.Search,
            ct);

        var items = page.Items
            .Select(tc => new TrainingClassListItemDto(
                tc.Id,
                tc.CourseId,
                tc.Title,
                tc.Status.ToString(),
                tc.MaxLearners,
                tc.CreatedAt))
            .ToList();

        return PagedList<TrainingClassListItemDto>.Create(items, page.Page, page.PageSize, page.TotalCount);
    }
}
