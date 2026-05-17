using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.QuizAggregate;

public sealed class Question : SoftDeletableEntity
{
    private Question() { }

    public Guid QuizId { get; private set; }
    public string Text { get; private set; } = default!;
    public QuestionType Type { get; private set; }
    public int Points { get; private set; }
    public int SortOrder { get; private set; }

    public List<QuestionOption> Options { get; private set; } = [];

    internal static Question Create(Guid quizId, string text, QuestionType type, int points, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Question text is required.");
        if (points < 0)
            throw new DomainException("Points must be non-negative.");

        return new Question
        {
            Id = Guid.NewGuid(),
            QuizId = quizId,
            Text = text.Trim(),
            Type = type,
            Points = points,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string text, QuestionType type, int points, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Question text is required.");
        if (points < 0)
            throw new DomainException("Points must be non-negative.");

        Text = text.Trim();
        Type = type;
        Points = points;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public QuestionOption AddOption(string text, bool isCorrect, int sortOrder)
    {
        var option = QuestionOption.Create(Id, text, isCorrect, sortOrder);
        Options.Add(option);
        UpdatedAt = DateTime.UtcNow;
        return option;
    }

    public void RemoveOption(Guid optionId)
    {
        var option = Options.FirstOrDefault(o => o.Id == optionId);
        if (option is null)
            throw new DomainException("Option not found.");

        option.MarkDeleted();
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        foreach (var option in Options)
        {
            option.MarkDeleted();
        }
    }
}
