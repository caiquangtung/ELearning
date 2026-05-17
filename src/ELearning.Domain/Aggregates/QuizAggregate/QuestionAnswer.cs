using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.QuizAggregate;

public sealed class QuestionAnswer : Entity
{
    private QuestionAnswer() { }

    public Guid QuizAttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid? SelectedOptionId { get; private set; }
    public string? TextAnswer { get; private set; }
    public int? Score { get; private set; }
    public bool? IsCorrect { get; private set; }
    public DateTime? GradedAt { get; private set; }
    public string? GradedBy { get; private set; }

    internal static QuestionAnswer Create(Guid quizAttemptId, Guid questionId, Guid? selectedOptionId, string? textAnswer)
    {
        return new QuestionAnswer
        {
            Id = Guid.NewGuid(),
            QuizAttemptId = quizAttemptId,
            QuestionId = questionId,
            SelectedOptionId = selectedOptionId,
            TextAnswer = string.IsNullOrWhiteSpace(textAnswer) ? null : textAnswer.Trim(),
        };
    }

    public void Update(Guid? selectedOptionId, string? textAnswer)
    {
        SelectedOptionId = selectedOptionId;
        TextAnswer = string.IsNullOrWhiteSpace(textAnswer) ? null : textAnswer.Trim();
    }

    public void Grade(int score, bool? isCorrect, string gradedBy)
    {
        Score = score;
        IsCorrect = isCorrect;
        GradedAt = DateTime.UtcNow;
        GradedBy = gradedBy;
    }
}
