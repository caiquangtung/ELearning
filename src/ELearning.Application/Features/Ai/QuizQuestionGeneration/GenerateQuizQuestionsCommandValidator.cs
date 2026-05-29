using FluentValidation;

namespace ELearning.Application.Features.Ai.QuizQuestionGeneration;

public sealed class GenerateQuizQuestionsCommandValidator : AbstractValidator<GenerateQuizQuestionsCommand>
{
    private static readonly string[] AllowedTypes = ["MultipleChoice", "Essay", "Code"];
    private static readonly string[] AllowedDifficulties = ["Easy", "Medium", "Hard"];

    public GenerateQuizQuestionsCommandValidator()
    {
        RuleFor(x => x.CourseId).NotEmpty();
        RuleFor(x => x.QuestionCount).InclusiveBetween(1, 10);
        RuleFor(x => x.Difficulty)
            .NotEmpty()
            .Must(d => AllowedDifficulties.Contains(d, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Difficulty must be Easy, Medium, or Hard.");
        RuleFor(x => x.QuestionTypes).NotEmpty();
        RuleForEach(x => x.QuestionTypes)
            .Must(t => AllowedTypes.Contains(t, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Question type must be MultipleChoice, Essay, or Code.");
    }
}
