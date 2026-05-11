using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.UpdateQuestion;

public sealed record UpdateQuestionCommand(
    Guid QuizId,
    Guid QuestionId,
    string Text,
    string Type,
    int Points,
    int SortOrder)
    : IRequest<Result<QuestionDto>>;
