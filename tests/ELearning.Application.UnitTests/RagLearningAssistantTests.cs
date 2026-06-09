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
    public void Local_dense_embedding_is_deterministic_fixed_size_and_normalized()
    {
        var service = new LocalDenseTextEmbeddingService();

        var first = service.Embed("JWT validation checks signatures, issuer, audience, and expiry.");
        var second = service.Embed("JWT validation checks signatures, issuer, audience, and expiry.");

        first.Vector.Should().HaveCount(LocalDenseTextEmbeddingService.EmbeddingDimensions);
        first.Vector.Should().Equal(second.Vector);
        first.Dimensions.Should().Be(384);
        first.Provider.Should().Be("Local");

        var norm = Math.Sqrt(first.Vector.Sum(x => x * x));
        norm.Should().BeApproximately(1d, 0.0001d);
    }

    [Fact]
    public async Task Knowledge_reindex_channel_preserves_job_id()
    {
        var channel = new InMemoryAiKnowledgeReindexChannel();
        var jobId = Guid.NewGuid();

        await channel.WriteAsync(jobId);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        await foreach (var queuedJobId in channel.ReadAllAsync(cts.Token))
        {
            queuedJobId.Should().Be(jobId);
            return;
        }

        throw new InvalidOperationException("AI knowledge reindex channel did not yield an item.");
    }

    [Fact]
    public void Knowledge_reindex_job_tracks_status_transitions()
    {
        var job = AiKnowledgeReindexJob.Create(Guid.NewGuid(), Guid.NewGuid());

        job.Status.Should().Be(AiKnowledgeReindexJobStatus.Queued);

        job.MarkInProgress();
        job.Status.Should().Be(AiKnowledgeReindexJobStatus.InProgress);
        job.StartedAt.Should().NotBeNull();

        job.MarkSucceeded(2, 12, 3);
        job.Status.Should().Be(AiKnowledgeReindexJobStatus.Succeeded);
        job.CompletedAt.Should().NotBeNull();
        job.IndexedCourses.Should().Be(2);
        job.IndexedChunks.Should().Be(12);
        job.DeletedStaleChunks.Should().Be(3);
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
