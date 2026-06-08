using ELearning.Application.Common.Interfaces;
using ELearning.Core.Constants;
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
    public void Extractive_answer_returns_only_retrieved_citations()
    {
        var citation = new AiChatCitation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Secure API Development",
            "Authentication",
            "JWT validation",
            "JWT validation checks signatures and expiry.",
            0.91m);

        var answer = AiRagChatService.BuildExtractiveAnswer(
            "How should JWT validation work?",
            [citation],
            "rag-learning-assistant-v1");

        answer.UsedContext.Should().BeTrue();
        answer.Citations.Should().ContainSingle().Which.Should().Be(citation);
        answer.Answer.Should().Contain(citation.Snippet);
        answer.Provider.Should().Be("Local");
    }

    [Theory]
    [InlineData(Roles.Admin, true)]
    [InlineData(Roles.Instructor, true)]
    [InlineData(Roles.OrgAdmin, true)]
    [InlineData(Roles.Learner, false)]
    public void Knowledge_access_policy_keeps_privileged_scope_explicit(string role, bool expected)
    {
        AiKnowledgeAccessPolicy.HasPrivilegedKnowledgeAccess([role]).Should().Be(expected);
    }

    [Fact]
    public async Task Knowledge_reindex_queue_preserves_course_scope()
    {
        var queue = new InMemoryAiKnowledgeReindexQueue();
        var courseId = Guid.NewGuid();

        await queue.EnqueueAsync(courseId);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await foreach (var queuedCourseId in queue.ReadAllAsync(cts.Token))
        {
            queuedCourseId.Should().Be(courseId);
            return;
        }

        throw new InvalidOperationException("AI knowledge reindex queue did not yield an item.");
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
