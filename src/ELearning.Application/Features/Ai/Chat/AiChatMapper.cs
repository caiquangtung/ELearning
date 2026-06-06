using ELearning.Application.Common.Interfaces;

namespace ELearning.Application.Features.Ai.Chat;

internal static class AiChatMapper
{
    public static AiChatSessionDto ToDto(AiChatSessionSummary session) =>
        new(session.Id, session.Title, session.CourseId, session.CreatedAt, session.UpdatedAt);

    public static AiChatMessageDto ToDto(AiChatMessageItem message) =>
        new(
            message.Id,
            message.Role,
            message.Content,
            message.Citations.Select(ToDto).ToList(),
            message.Provider,
            message.Model,
            message.PromptVersion,
            message.Confidence,
            message.UsedContext,
            message.CreatedAt);

    public static AiChatAnswerDto ToDto(AiChatAnswer answer) =>
        new(
            answer.MessageId,
            answer.Answer,
            answer.Citations.Select(ToDto).ToList(),
            answer.Confidence,
            answer.UsedContext,
            answer.Provider,
            answer.Model);

    private static AiChatCitationDto ToDto(AiChatCitation citation) =>
        new(
            citation.ChunkId,
            citation.CourseId,
            citation.SectionId,
            citation.LessonId,
            citation.CourseTitle,
            citation.SectionTitle,
            citation.LessonTitle,
            citation.Snippet,
            citation.Score);
}
