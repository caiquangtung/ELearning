namespace ELearning.WebApi.Contracts.v1;

public sealed record GenerateQuizQuestionsRequest(
    Guid CourseId,
    Guid? LessonId,
    int QuestionCount,
    string Difficulty,
    IReadOnlyList<string> QuestionTypes);

public sealed record CreateAiChatSessionRequest(Guid? CourseId, string? Title);

public sealed record SendAiChatMessageRequest(string Message);

public sealed record ReindexAiKnowledgeRequest(Guid? CourseId);
