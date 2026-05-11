using ELearning.Application.Features.Quizzes.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.QuizAggregate;
using MediatR;

namespace ELearning.Application.Features.Quizzes.AddQuestion;

public sealed class AddQuestionCommandHandler(
    IQuizRepository quizRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddQuestionCommand, Result<QuestionDto>>
{
    public async Task<Result<QuestionDto>> Handle(AddQuestionCommand request, CancellationToken ct)
    {
        var quiz = await quizRepository.GetByIdWithQuestionsAsync(request.QuizId, ct);
        if (quiz is null)
            return Result.Failure<QuestionDto>(Error.NotFound("Quiz", request.QuizId));

        if (!Enum.TryParse<QuestionType>(request.Type, true, out var questionType))
            return Result.Failure<QuestionDto>(Error.Validation("Question.Type", "Invalid question type. Use MultipleChoice, Essay, or Code."));

        var question = quiz.AddQuestion(request.Text, questionType, request.Points, request.SortOrder);

        foreach (var option in request.Options)
        {
            question.AddOption(option.Text, option.IsCorrect, option.SortOrder);
        }

        quizRepository.Update(quiz);
        await unitOfWork.SaveChangesAsync(ct);

        return new QuestionDto(
            question.Id,
            question.Text,
            question.Type.ToString(),
            question.Points,
            question.SortOrder,
            question.Options
                .Where(o => !o.IsDeleted)
                .OrderBy(o => o.SortOrder)
                .Select(o => new QuestionOptionDto(o.Id, o.Text, o.IsCorrect, o.SortOrder))
                .ToList());
    }
}
