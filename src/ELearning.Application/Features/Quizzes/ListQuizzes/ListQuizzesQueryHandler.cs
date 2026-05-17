using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.ListQuizzes;

public sealed class ListQuizzesQueryHandler(IQuizRepository quizRepository)
    : IRequestHandler<ListQuizzesQuery, Result<PagedList<QuizListItemDto>>>
{
    public async Task<Result<PagedList<QuizListItemDto>>> Handle(ListQuizzesQuery request, CancellationToken ct)
    {
        var parsedStatus = Enum.TryParse(request.Status, true, out QuizStatus s)
            ? s
            : (QuizStatus?)null;

        var paged = await quizRepository.ListAsync(request.Page, request.PageSize, request.Search, parsedStatus, ct);

        var dtos = paged.Items.Select(q => new QuizListItemDto(
            q.Id,
            q.Title,
            q.Status.ToString(),
            q.Questions.Count(qn => !qn.IsDeleted),
            q.CreatedAt)).ToList();

        return PagedList<QuizListItemDto>.Create(dtos, paged.Page, paged.PageSize, paged.TotalCount);
    }
}
