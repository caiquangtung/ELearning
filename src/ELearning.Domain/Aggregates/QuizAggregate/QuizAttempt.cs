using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.QuizAggregate;

public sealed class QuizAttempt : AuditableEntity
{
    private QuizAttempt() { }

    public Guid QuizId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public AttemptStatus Status { get; private set; }
    public int? TotalScore { get; private set; }

    public List<QuestionAnswer> Answers { get; private set; } = [];

    public static QuizAttempt Start(Guid quizId, Guid userId)
    {
        return new QuizAttempt
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Status = AttemptStatus.InProgress,
            CreatedAt = DateTime.UtcNow
        };
    }

    public QuestionAnswer AddAnswer(Guid questionId, Guid? selectedOptionId, string? textAnswer)
    {
        if (Status != AttemptStatus.InProgress)
            throw new DomainException("Cannot modify answers after submission.");

        var existing = Answers.FirstOrDefault(a => a.QuestionId == questionId);
        if (existing is not null)
        {
            existing.Update(selectedOptionId, textAnswer);
            UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var answer = QuestionAnswer.Create(Id, questionId, selectedOptionId, textAnswer);
        Answers.Add(answer);
        UpdatedAt = DateTime.UtcNow;
        return answer;
    }

    public void Submit()
    {
        if (Status != AttemptStatus.InProgress)
            throw new DomainException("Attempt has already been submitted.");

        Status = AttemptStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetScore(int totalScore)
    {
        if (totalScore < 0)
            throw new DomainException("Score must be non-negative.");

        TotalScore = totalScore;
        Status = AttemptStatus.Graded;
        UpdatedAt = DateTime.UtcNow;
    }
}
