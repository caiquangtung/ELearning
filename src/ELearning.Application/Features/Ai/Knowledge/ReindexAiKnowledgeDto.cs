namespace ELearning.Application.Features.Ai.Knowledge;

public sealed record ReindexAiKnowledgeDto(
    int IndexedCourses,
    int IndexedChunks,
    int DeletedStaleChunks);
