using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Ai;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class RagLearningAssistantTests
{
    [Fact]
    public void Chunker_splits_content_with_stable_chunk_order_and_size()
    {
        var course = Course.Create("Secure API Development", "Learn authentication and authorization.");
        var section = course.AddSection("Authentication");
        var lesson = section.AddLesson("JWT validation");
        lesson.UpdateContent(string.Join(' ', Enumerable.Repeat(
            "JWT validation checks signatures, issuer, audience, expiry, and permission claims.", 30)));
        course.Publish();

        var chunker = new AiKnowledgeChunker();
        var first = chunker.BuildCourseChunks(course, 500);
        var second = chunker.BuildCourseChunks(course, 500);

        first.Should().NotBeEmpty();
        first.Select(x => x.Text).Should().Equal(second.Select(x => x.Text));
        first.Should().OnlyContain(x => x.Text.Length <= 500);
        first.Select(x => x.ChunkIndex).Should().ContainInOrder(0, 1);
    }

    [Fact]
    public void No_context_answer_refuses_without_citations()
    {
        var answer = AiRagChatService.BuildNoContextAnswer(
            "Who won the football match?",
            "rag-learning-assistant-v1");

        answer.UsedContext.Should().BeFalse();
        answer.Citations.Should().BeEmpty();
        answer.Confidence.Should().Be(0);
        answer.Answer.Should().Contain("don't have enough course material");
    }

    [Fact]
    public void Chat_message_rejects_invalid_confidence()
    {
        var act = () => AiChatMessage.Assistant(
            Guid.NewGuid(),
            "Answer",
            "[]",
            "Local",
            "extractive-rag-v1",
            "rag-learning-assistant-v1",
            1.2m,
            true);

        act.Should().Throw<Exception>().WithMessage("*confidence*");
    }
}
