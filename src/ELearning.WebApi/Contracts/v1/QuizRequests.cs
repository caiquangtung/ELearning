namespace ELearning.WebApi.Contracts.v1;

public sealed record ListQuizzesRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null);

public sealed record GetAttemptRequest(Guid UserId);

public sealed record CreateQuizRequest(
    Guid? CourseId,
    Guid? LessonId,
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? PassingScore);

public sealed record UpdateQuizRequest(
    string Title,
    string? Description,
    int? TimeLimitMinutes,
    int? PassingScore);

public sealed record AddQuestionRequest(
    string Text,
    string Type,
    int Points,
    int SortOrder,
    List<AddQuestionOptionRequest>? Options);

public sealed record AddQuestionOptionRequest(string Text, bool IsCorrect, int SortOrder);

public sealed record UpdateQuestionRequest(
    string Text,
    string Type,
    int Points,
    int SortOrder);

public sealed record StartAttemptRequest(Guid UserId);

public sealed record SubmitAttemptRequest(
    Guid UserId,
    List<AnswerSubmissionRequest>? Answers);

public sealed record AnswerSubmissionRequest(
    Guid QuestionId,
    Guid? SelectedOptionId,
    string? TextAnswer);

public sealed record GradeAttemptRequest(List<QuestionGradeRequest>? Grades);

public sealed record QuestionGradeRequest(Guid QuestionId, int Score, bool? IsCorrect);

public sealed record SuggestEssayGradesRequest(string? Rubric);
