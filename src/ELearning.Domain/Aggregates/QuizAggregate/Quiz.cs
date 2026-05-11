using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.QuizAggregate;

public sealed class Quiz : SoftDeletableAggregateRoot
{
    private Quiz() { }

    public Guid? CourseId { get; private set; }
    public Guid? LessonId { get; private set; }
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public int? TimeLimitMinutes { get; private set; }
    public int? PassingScore { get; private set; }
    public QuizStatus Status { get; private set; }

    public List<Question> Questions { get; private set; } = [];

    public static Quiz CreateForCourse(Guid courseId, string title, string? description, int? timeLimitMinutes, int? passingScore)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Quiz title is required.");
        if (timeLimitMinutes.HasValue && timeLimitMinutes.Value < 1)
            throw new DomainException("Time limit must be at least 1 minute.");
        if (passingScore.HasValue && passingScore.Value < 0)
            throw new DomainException("Passing score must be non-negative.");

        return new Quiz
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            TimeLimitMinutes = timeLimitMinutes,
            PassingScore = passingScore,
            Status = QuizStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Quiz CreateForLesson(Guid lessonId, string title, string? description, int? timeLimitMinutes, int? passingScore)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Quiz title is required.");
        if (timeLimitMinutes.HasValue && timeLimitMinutes.Value < 1)
            throw new DomainException("Time limit must be at least 1 minute.");
        if (passingScore.HasValue && passingScore.Value < 0)
            throw new DomainException("Passing score must be non-negative.");

        return new Quiz
        {
            Id = Guid.NewGuid(),
            LessonId = lessonId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            TimeLimitMinutes = timeLimitMinutes,
            PassingScore = passingScore,
            Status = QuizStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string? description, int? timeLimitMinutes, int? passingScore)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Quiz title is required.");
        if (timeLimitMinutes.HasValue && timeLimitMinutes.Value < 1)
            throw new DomainException("Time limit must be at least 1 minute.");
        if (passingScore.HasValue && passingScore.Value < 0)
            throw new DomainException("Passing score must be non-negative.");

        Title = title.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        TimeLimitMinutes = timeLimitMinutes;
        PassingScore = passingScore;
        UpdatedAt = DateTime.UtcNow;
    }

    public Question AddQuestion(string text, QuestionType type, int points, int sortOrder)
    {
        var question = Question.Create(Id, text, type, points, sortOrder);
        Questions.Add(question);
        UpdatedAt = DateTime.UtcNow;
        return question;
    }

    public void RemoveQuestion(Guid questionId)
    {
        var question = Questions.FirstOrDefault(q => q.Id == questionId);
        if (question is null)
            throw new DomainException("Question not found.");

        question.MarkDeleted();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status == QuizStatus.Published) return;
        if (Questions.Count == 0)
            throw new DomainException("Quiz must have at least one question before publishing.");
        if (Questions.Any(q => q.Type == QuestionType.MultipleChoice && q.Options.Count == 0))
            throw new DomainException("All multiple-choice questions must have at least one option.");

        Status = QuizStatus.Published;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Archive()
    {
        if (Status == QuizStatus.Archived) return;
        Status = QuizStatus.Archived;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        if (Status == QuizStatus.Draft) return;
        Status = QuizStatus.Draft;
        UpdatedAt = DateTime.UtcNow;
    }
}
