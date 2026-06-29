using ELearning.Application.Common.Interfaces;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

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
    public async Task Local_dense_embedding_is_deterministic_fixed_size_and_normalized()
    {
        var service = new LocalDenseTextEmbeddingService();

        var first = await service.EmbedAsync("JWT validation checks signatures, issuer, audience, and expiry.");
        var second = await service.EmbedAsync("JWT validation checks signatures, issuer, audience, and expiry.");

        first.Vector.Should().HaveCount(LocalDenseTextEmbeddingService.EmbeddingDimensions);
        first.Vector.Should().Equal(second.Vector);
        first.Dimensions.Should().Be(384);
        first.Provider.Should().Be("Local");

        var norm = Math.Sqrt(first.Vector.Sum(x => x * x));
        norm.Should().BeApproximately(1d, 0.0001d);
    }

    [Fact]
    public async Task OpenAi_compatible_embedding_normalizes_valid_vector()
    {
        var response = $$"""
            {
              "model": "test-embedding",
              "data": [
                { "embedding": [{{string.Join(',', Enumerable.Repeat("1", 384))}}] }
              ]
            }
            """;
        var service = new OpenAiCompatibleTextEmbeddingService(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.OK, response)),
            Options.Create(new AiOptions
            {
                RagEmbeddingProvider = "OpenAiCompatible",
                RagEmbeddingApiKey = "test-key",
                RagEmbeddingModel = "test-embedding",
                RagEmbeddingDimensions = 384,
                RagEmbeddingMaxRetries = 0
            }));

        var embedding = await service.EmbedAsync("JWT validation");

        embedding.Provider.Should().Be("OpenAiCompatible");
        embedding.Model.Should().Be("test-embedding");
        embedding.Vector.Should().HaveCount(384);
        EmbeddingVectorUtils.Norm(embedding.Vector).Should().BeApproximately(1d, 0.0001d);
    }

    [Fact]
    public async Task OpenAi_compatible_embedding_rejects_wrong_dimension()
    {
        const string response = """{ "model": "bad", "data": [ { "embedding": [0.1, 0.2, 0.3] } ] }""";
        var service = new OpenAiCompatibleTextEmbeddingService(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.OK, response)),
            Options.Create(new AiOptions
            {
                RagEmbeddingProvider = "OpenAiCompatible",
                RagEmbeddingApiKey = "test-key",
                RagEmbeddingModel = "bad",
                RagEmbeddingDimensions = 384,
                RagEmbeddingMaxRetries = 0
            }));

        var act = () => service.EmbedAsync("JWT validation");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expected 384*");
    }

    [Fact]
    public async Task Configurable_embedding_falls_back_to_local_when_provider_fails()
    {
        var options = Options.Create(new AiOptions
        {
            RagEmbeddingProvider = "OpenAiCompatible",
            RagEmbeddingApiKey = "test-key",
            RagEmbeddingModel = "test-embedding",
            RagEmbeddingDimensions = 384,
            RagEmbeddingMaxRetries = 0,
            FallbackToLocal = true
        });
        var local = new LocalDenseTextEmbeddingService();
        var remote = new OpenAiCompatibleTextEmbeddingService(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.InternalServerError, "{}")),
            options);
        var service = new ConfigurableAiTextEmbeddingService(
            local,
            remote,
            options,
            NullLogger<ConfigurableAiTextEmbeddingService>.Instance);

        var embedding = await service.EmbedAsync("JWT validation");

        embedding.Provider.Should().Be("Local");
        embedding.Vector.Should().HaveCount(384);
    }

    [Fact]
    public void Retriever_lexical_boost_can_promote_relevant_candidates()
    {
        var candidate = new AiKnowledgeRetriever.VectorCandidate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Secure Coding Fundamentals",
            "Authentication",
            "JWT validation",
            "Lesson",
            0,
            "JWT validation checks signatures, issuer, audience, and expiry.",
            0.02m);

        var citations = AiKnowledgeRetriever.BuildCitations(
            "How does JWT validation check signatures?",
            [candidate],
            0.05m,
            4,
            800);

        citations.Should().ContainSingle();
        citations[0].Score.Should().BeGreaterThan(0.05m);
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
    public void Rag_evaluation_run_tracks_quality_metrics()
    {
        var run = AiRagEvaluationRun.Succeeded(
            Guid.NewGuid(),
            "rag-golden-v1",
            4,
            3,
            0.75m,
            1m,
            1m,
            0.5m,
            DateTime.UtcNow.AddSeconds(-2));

        run.Status.Should().Be(AiRagEvaluationRunStatus.Succeeded);
        run.TotalCases.Should().Be(4);
        run.PassedCases.Should().Be(3);
        run.RetrievalHitRate.Should().Be(0.75m);
        run.CitationValidityRate.Should().Be(1m);
        run.GroundednessRate.Should().Be(0.5m);
        run.CompletedAt.Should().NotBeNull();
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

    private sealed class StaticResponseHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
    }
}
