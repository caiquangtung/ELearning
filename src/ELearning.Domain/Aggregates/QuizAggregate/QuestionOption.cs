using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.QuizAggregate;

public sealed class QuestionOption : SoftDeletableEntity
{
    private QuestionOption() { }

    public Guid QuestionId { get; private set; }
    public string Text { get; private set; } = default!;
    public bool IsCorrect { get; private set; }
    public int SortOrder { get; private set; }

    internal static QuestionOption Create(Guid questionId, string text, bool isCorrect, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Option text is required.");

        return new QuestionOption
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            Text = text.Trim(),
            IsCorrect = isCorrect,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string text, bool isCorrect, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new DomainException("Option text is required.");

        Text = text.Trim();
        IsCorrect = isCorrect;
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
}
