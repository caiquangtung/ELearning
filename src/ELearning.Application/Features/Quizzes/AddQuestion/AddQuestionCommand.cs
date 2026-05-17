using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Quizzes.AddQuestion;

public sealed record AddQuestionCommand(
    Guid QuizId,
    string Text,
    string Type,
    int Points,
    int SortOrder,
    IReadOnlyList<AddQuestionOptionDto> Options)
    : IRequest<Result<QuestionDto>>;

public sealed record AddQuestionOptionDto(string Text, bool IsCorrect, int SortOrder);
