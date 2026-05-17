using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.ListQuizzes;

public sealed record ListQuizzesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null)
    : IRequest<Result<PagedList<QuizListItemDto>>>;
