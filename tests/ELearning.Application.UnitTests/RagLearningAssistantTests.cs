using ELearning.Application.Common.Interfaces;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Ai;
using ELearning.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Threading;

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
    public void Greeting_answer_skips_retrieval_and_returns_friendly_message()
    {
        var gate = BuildIntentGate();
        var intent = gate.Evaluate("Hello there!");

        intent.SkipRetrieval.Should().BeTrue();
        intent.IsGreeting.Should().BeTrue();

        var answer = AiRagChatService.BuildGreetingAnswer("rag-learning-assistant-v1");
        answer.UsedContext.Should().BeFalse();
        answer.Citations.Should().BeEmpty();
        answer.Answer.Should().Contain("AI learning assistant");
        answer.Provider.Should().Be("Local");
    }

    [Theory]
    [InlineData("hi")]
    [InlineData("Hey")]
    [InlineData("Good morning")]
    [InlineData("what's up")]
    [InlineData("howdy")]
    [InlineData("yo")]
    [InlineData("what is your name")]
    [InlineData("Who are you")]
    public void Greeting_detection_matches_common_greetings(string message)
    {
        var gate = BuildIntentGate();
        var intent = gate.Evaluate(message);

        intent.SkipRetrieval.Should().BeTrue();
        intent.IsGreeting.Should().BeTrue();
    }

    [Fact]
    public void Irrelevant_question_skips_retrieval_and_returns_no_context_message()
    {
        var gate = BuildIntentGate();
        var intent = gate.Evaluate("okay nice");

        intent.SkipRetrieval.Should().BeTrue();
        intent.IsGreeting.Should().BeFalse();
        intent.Reason.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData("????")]
    [InlineData("ok")]
    [InlineData("yes no")]
    public void Short_or_empty_messages_are_marked_irrelevant(string message)
    {
        var gate = BuildIntentGate();
        var intent = gate.Evaluate(message);

        intent.SkipRetrieval.Should().BeTrue();
        intent.IsGreeting.Should().BeFalse();
    }

    [Fact]
    public void Intent_gate_can_be_disabled_via_options()
    {
        var gate = BuildIntentGate(enabled: false);
        var intent = gate.Evaluate("Hello!");

        intent.SkipRetrieval.Should().BeFalse();
        intent.IsGreeting.Should().BeFalse();
    }

    [Fact]
    public void Substantive_questions_pass_greeting_and_irrelevant_checks()
    {
        var gate = BuildIntentGate();
        var intent = gate.Evaluate("How does JWT validation work in this course?");

        intent.SkipRetrieval.Should().BeFalse();
        intent.IsGreeting.Should().BeFalse();
        intent.Reason.Should().BeNull();
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

    [Fact]
    public void Configured_no_context_answer_uses_custom_response()
    {
        var answer = AiRagChatService.BuildNoContextAnswer(
            "What is outside this course?",
            "rag-learning-assistant-v1",
            "Please ask about indexed course material.");

        answer.Answer.Should().Be("Please ask about indexed course material.");
        answer.UsedContext.Should().BeFalse();
        answer.PromptVersion.Should().Be("rag-learning-assistant-v1");
    }

    [Fact]
    public void Provider_json_answer_clamps_confidence_and_keeps_prompt_version()
    {
        var citation = new AiChatCitation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Secure API Development",
            null,
            null,
            "JWT validation checks signatures.",
            0.91m);

        var answer = AiRagChatService.TryBuildProviderJsonAnswer(
            "How does JWT validation work?",
            "OpenAiCompatible",
            "gpt-test",
            """{"answer":"JWT validation checks signatures.","confidence":1.25}""",
            42,
            [citation],
            "rag-learning-assistant-v1");

        answer.Should().NotBeNull();
        answer!.Confidence.Should().Be(1m);
        answer.PromptVersion.Should().Be("rag-learning-assistant-v1");
        answer.Citations.Should().ContainSingle().Which.Should().Be(citation);
        answer.TokenEstimate.Should().Be(42);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("""{"confidence":0.8}""")]
    public void Provider_json_answer_rejects_malformed_or_empty_payload(string content)
    {
        var answer = AiRagChatService.TryBuildProviderJsonAnswer(
            "Question",
            "GoogleAiStudio",
            "gemini-test",
            content,
            null,
            [],
            "rag-learning-assistant-v1");

        answer.Should().BeNull();
    }

    [Fact]
    public void Retrieval_quality_gate_requires_citation_above_threshold()
    {
        var weak = new AiChatCitation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Course",
            null,
            null,
            "Weak citation.",
            0.49m);

        AiRagChatService.HasSufficientRetrievalContext([], 0.50m).Should().BeFalse();
        AiRagChatService.HasSufficientRetrievalContext([weak], 0.50m).Should().BeFalse();
        AiRagChatService.HasSufficientRetrievalContext([weak with { Score = 0.50m }], 0.50m).Should().BeTrue();
    }

    [Fact]
    public void Retriever_threshold_0_70_filters_weak_candidates_after_lexical_boost()
    {
        var weak = new AiKnowledgeRetriever.VectorCandidate(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "Course A", "Section A", "Lesson A", "Lesson", 0,
            "Random text with no overlap.", 0.10m);

        var citations = AiKnowledgeRetriever.BuildCitations(
            "How does JWT validation check signatures?",
            [weak],
            0.70m,
            4,
            800);

        citations.Should().BeEmpty();
    }

    [Fact]
    public void Retriever_lexical_boost_can_promote_relevant_candidates_above_threshold()
    {
        var candidate = new AiKnowledgeRetriever.VectorCandidate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Secure Coding Fundamentals",
            "Authentication",
            "JWT validation signatures authority check",
            "Lesson",
            0,
            "JWT validation checks signatures and issuer authority.",
            0.65m);

        var citations = AiKnowledgeRetriever.BuildCitations(
            "How does JWT validation check signatures and issuer authority?",
            [candidate],
            0.70m,
            4,
            800);

        citations.Should().ContainSingle();
        citations[0].Score.Should().BeGreaterThan(0.70m);
        citations[0].RawScore.Should().Be(0.65m);
    }

    [Fact]
    public void Hybrid_fusion_keeps_dense_candidate_and_marks_dense_sparse_overlap()
    {
        var chunkId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var dense = new AiKnowledgeRetriever.VectorCandidate(
            chunkId,
            courseId,
            null,
            null,
            "Secure Coding",
            null,
            null,
            "Lesson",
            0,
            "JWT validation checks signatures.",
            0.78m,
            "Dense");
        var sparse = dense with { Score = 0.45m, RetrievalSource = "Sparse" };

        var fused = AiKnowledgeRetriever.FuseCandidates([dense], [sparse], 10);

        fused.Should().ContainSingle();
        fused[0].ChunkId.Should().Be(chunkId);
        fused[0].RetrievalSource.Should().Be("Hybrid");
        fused[0].Score.Should().BeGreaterThan(dense.Score);
    }

    [Fact]
    public void Hybrid_fusion_can_promote_strong_sparse_candidate()
    {
        var weakDense = new AiKnowledgeRetriever.VectorCandidate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Random Course",
            null,
            null,
            "Lesson",
            0,
            "Unrelated text.",
            0.20m,
            "Dense");
        var strongSparse = new AiKnowledgeRetriever.VectorCandidate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Secure Coding",
            null,
            null,
            "Lesson",
            0,
            "JWT validation checks signatures.",
            0.86m,
            "Sparse");

        var fused = AiKnowledgeRetriever.FuseCandidates([weakDense], [strongSparse], 10);

        fused[0].ChunkId.Should().Be(strongSparse.ChunkId);
        fused[0].RetrievalSource.Should().Be("Sparse");
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

        first.Vector.Should().HaveCount(LocalDenseTextEmbeddingService.DefaultEmbeddingDimensions);
        first.Vector.Should().Equal(second.Vector);
        first.Dimensions.Should().Be(768);
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
        embedding.Vector.Should().HaveCount(768);
    }

    [Fact]
    public async Task Google_ai_studio_embedding_sends_document_task_type_title_and_dimensions()
    {
        var response = $$"""
            {
              "embedding": {
                "values": [{{string.Join(',', Enumerable.Repeat("1", 768))}}]
              }
            }
            """;
        var handler = new StaticResponseHandler(HttpStatusCode.OK, response);
        var service = new GoogleAiStudioTextEmbeddingService(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagEmbeddingProvider = "GoogleAiStudio",
                RagEmbeddingApiKey = "test-key",
                RagEmbeddingModel = "gemini-embedding-2",
                RagEmbeddingDimensions = 768,
                RagEmbeddingMaxRetries = 0
            }));

        var embedding = await service.EmbedAsync(new AiTextEmbeddingRequest(
            "JWT validation checks signatures.",
            AiTextEmbeddingPurpose.RetrievalDocument,
            "Secure API Development - JWT validation"));

        embedding.Provider.Should().Be("GoogleAiStudio");
        embedding.Model.Should().Be("models/gemini-embedding-2");
        embedding.Vector.Should().HaveCount(768);
        EmbeddingVectorUtils.Norm(embedding.Vector).Should().BeApproximately(1d, 0.0001d);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Headers.GetValues("x-goog-api-key").Should().ContainSingle("test-key");
        handler.LastRequest.RequestUri!.ToString().Should().Contain("models/gemini-embedding-2:embedContent");
        handler.LastRequestBody.Should().Contain("\"taskType\":\"RETRIEVAL_DOCUMENT\"");
        handler.LastRequestBody.Should().Contain("\"title\":\"Secure API Development - JWT validation\"");
        handler.LastRequestBody.Should().Contain("\"outputDimensionality\":768");
        handler.LastRequestBody.Should().Contain("\"text\":\"JWT validation checks signatures.\"");
    }

    [Fact]
    public async Task Google_ai_studio_embedding_sends_query_task_type_without_title()
    {
        var response = $$"""
            {
              "embedding": {
                "values": [{{string.Join(',', Enumerable.Repeat("1", 768))}}]
              }
            }
            """;
        var handler = new StaticResponseHandler(HttpStatusCode.OK, response);
        var service = new GoogleAiStudioTextEmbeddingService(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagEmbeddingProvider = "GoogleAiStudio",
                RagEmbeddingApiKey = "test-key",
                RagEmbeddingModel = "gemini-embedding-2",
                RagEmbeddingDimensions = 768,
                RagEmbeddingMaxRetries = 0
            }));

        await service.EmbedAsync(new AiTextEmbeddingRequest(
            "How does JWT validation work?",
            AiTextEmbeddingPurpose.RetrievalQuery,
            "Ignored title"));

        handler.LastRequestBody.Should().Contain("\"taskType\":\"RETRIEVAL_QUERY\"");
        handler.LastRequestBody.Should().NotContain("\"title\"");
    }

    [Fact]
    public async Task Google_ai_studio_embedding_rejects_wrong_dimension()
    {
        const string response = """{ "embedding": { "values": [0.1, 0.2, 0.3] } }""";
        var service = new GoogleAiStudioTextEmbeddingService(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.OK, response)),
            Options.Create(new AiOptions
            {
                RagEmbeddingProvider = "GoogleAiStudio",
                RagEmbeddingApiKey = "test-key",
                RagEmbeddingModel = "gemini-embedding-2",
                RagEmbeddingDimensions = 768,
                RagEmbeddingMaxRetries = 0
            }));

        var act = () => service.EmbedAsync(new AiTextEmbeddingRequest(
            "JWT validation",
            AiTextEmbeddingPurpose.RetrievalQuery));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expected 768*");
    }

    [Fact]
    public async Task Google_ai_studio_embedding_throws_provider_exception_on_rate_limit()
    {
        var service = new GoogleAiStudioTextEmbeddingService(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.TooManyRequests, "{}")),
            Options.Create(new AiOptions
            {
                RagEmbeddingProvider = "GoogleAiStudio",
                RagEmbeddingApiKey = "test-key",
                RagEmbeddingModel = "gemini-embedding-2",
                RagEmbeddingDimensions = 768,
                RagEmbeddingMaxRetries = 0
            }));

        var act = () => service.EmbedAsync(new AiTextEmbeddingRequest(
            "JWT validation",
            AiTextEmbeddingPurpose.RetrievalQuery));

        var exception = await act.Should().ThrowAsync<AiTextEmbeddingProviderException>();
        exception.Which.IsRetriable.Should().BeTrue();
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

    [Fact]
    public async Task Google_ai_studio_chat_sends_correct_request_format()
    {
        var response = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": "My name is Elearning Bot." }
                ]
              }
            }
          ],
          "modelVersion": "gemini-2.0-flash",
          "usageMetadata": {
            "totalTokenCount": 25
          }
        }
        """;
        var handler = new StaticResponseHandler(HttpStatusCode.OK, response);
        var client = new GoogleAiStudioChatClient(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagChatProvider = "GoogleAiStudio",
                RagChatModel = "gemini-2.0-flash",
                RagEmbeddingApiKey = "test-key",
                MaxOutputTokens = 1200,
                TimeoutSeconds = 30,
                MaxRetries = 2
            }));

        var result = await client.CompleteJsonAsync(
            "system prompt",
            "What is your name?",
            CancellationToken.None);

        result.Provider.Should().Be("GoogleAiStudio");
        result.Model.Should().Be("gemini-2.0-flash");
        result.Content.Should().Be("My name is Elearning Bot.");
        result.TokenEstimate.Should().Be(25);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.Headers.GetValues("x-goog-api-key").Should().ContainSingle("test-key");
        handler.LastRequest.RequestUri!.ToString().Should().Contain("models/gemini-2.0-flash:generateContent");
        handler.LastRequestBody.Should().Contain("\"systemInstruction\"");
        handler.LastRequestBody.Should().Contain("system prompt");
        handler.LastRequestBody.Should().Contain("\"parts\"");
        handler.LastRequestBody.Should().Contain("What is your name?");
    }

    [Fact]
    public async Task Google_ai_studio_chat_throws_on_empty_response()
    {
        var response = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  { "text": "" }
                ]
              }
            }
          ]
        }
        """;
        var client = new GoogleAiStudioChatClient(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.OK, response)),
            Options.Create(new AiOptions
            {
                RagChatProvider = "GoogleAiStudio",
                RagChatModel = "gemini-2.0-flash",
                RagEmbeddingApiKey = "test-key"
            }));

        var act = () => client.CompleteJsonAsync("system", "question", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*empty response*");
    }

    [Fact]
    public async Task Google_ai_studio_chat_retries_on_server_error()
    {
        var handler = new CountingHandler(HttpStatusCode.InternalServerError, "{}", 2);
        var client = new GoogleAiStudioChatClient(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagChatProvider = "GoogleAiStudio",
                RagChatModel = "gemini-2.0-flash",
                RagEmbeddingApiKey = "test-key",
                MaxRetries = 1,
                TimeoutSeconds = 30
            }));

        var act = () => client.CompleteJsonAsync("system", "question", CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        handler.Attempts.Should().Be(2);
    }

    [Fact]
    public async Task Ollama_chat_client_sends_correct_request_format()
    {
        var response = """
        {
          "model": "qwen2.5:7b",
          "message": {
            "role": "assistant",
            "content": "Hello, I am Ollama."
          },
          "prompt_eval_count": 10,
          "eval_count": 15
        }
        """;
        var handler = new StaticResponseHandler(HttpStatusCode.OK, response);
        var client = new OllamaChatClient(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagChatProvider = "Ollama",
                OllamaModel = "qwen2.5:7b",
                OllamaBaseUrl = "http://localhost:11434",
                MaxOutputTokens = 1200,
                TimeoutSeconds = 30,
                MaxRetries = 2
            }));

        var result = await client.CompleteJsonAsync(
            "system prompt",
            "What is your name?",
            ct: CancellationToken.None);

        result.Provider.Should().Be("Ollama");
        result.Model.Should().Be("qwen2.5:7b");
        result.Content.Should().Be("Hello, I am Ollama.");
        result.TokenEstimate.Should().Be(25);

        handler.LastRequest.Should().NotBeNull();
        handler.LastRequest!.RequestUri!.ToString().Should().Be("http://localhost:11434/api/chat");
        handler.LastRequestBody.Should().Contain("\"model\":\"qwen2.5:7b\"");
        handler.LastRequestBody.Should().Contain("\"format\":\"json\"");
        handler.LastRequestBody.Should().Contain("\"system\"");
        handler.LastRequestBody.Should().Contain("system prompt");
        handler.LastRequestBody.Should().Contain("\"user\"");
        handler.LastRequestBody.Should().Contain("What is your name?");
        handler.LastRequestBody.Should().Contain("\"num_predict\":1200");
    }

    [Fact]
    public async Task Ollama_chat_client_skips_json_format_when_force_json_is_false()
    {
        var response = """
        {
          "model": "qwen2.5:7b",
          "message": {
            "role": "assistant",
            "content": "Hello, I am Ollama."
          }
        }
        """;
        var handler = new StaticResponseHandler(HttpStatusCode.OK, response);
        var client = new OllamaChatClient(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagChatProvider = "Ollama",
                OllamaModel = "qwen2.5:7b",
                OllamaBaseUrl = "http://localhost:11434"
            }));

        var result = await client.CompleteJsonAsync(
            "system prompt",
            "What is your name?",
            false,
            CancellationToken.None);

        result.Content.Should().Be("Hello, I am Ollama.");
        handler.LastRequestBody.Should().NotContain("\"format\"");
    }

    [Fact]
    public async Task Ollama_chat_client_throws_on_empty_response()
    {
        var response = """
        {
          "model": "qwen2.5:7b",
          "message": {
            "role": "assistant",
            "content": ""
          }
        }
        """;
        var client = new OllamaChatClient(
            new HttpClient(new StaticResponseHandler(HttpStatusCode.OK, response)),
            Options.Create(new AiOptions
            {
                RagChatProvider = "Ollama",
                OllamaModel = "qwen2.5:7b",
                OllamaBaseUrl = "http://localhost:11434"
            }));

        var act = () => client.CompleteJsonAsync("system", "question", ct: CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*empty message*");
    }

    [Fact]
    public async Task Ollama_chat_client_retries_on_server_error()
    {
        var handler = new CountingHandlerForOllama(HttpStatusCode.InternalServerError, "{}", 2);
        var client = new OllamaChatClient(
            new HttpClient(handler),
            Options.Create(new AiOptions
            {
                RagChatProvider = "Ollama",
                OllamaModel = "qwen2.5:7b",
                OllamaBaseUrl = "http://localhost:11434",
                MaxRetries = 1,
                TimeoutSeconds = 30
            }));

        var act = () => client.CompleteJsonAsync("system", "question", ct: CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        handler.Attempts.Should().Be(2);
    }

    private static AiChatIntentGate BuildIntentGate(bool enabled = true) =>
        new(Options.Create(new AiOptions { RagEnableIntentGating = enabled }));

    private sealed class StaticResponseHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public string LastRequestBody { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestBody = request.Content is null
                ? ""
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class CountingHandler(HttpStatusCode statusCode, string body, int failCount) : HttpMessageHandler
    {
        public int Attempts { get; private set; }
        private readonly string _body = body;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Attempts++;
            if (Attempts <= failCount)
            {
                await Task.Delay(1, cancellationToken);
                return new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "candidates": [
                    {
                      "content": {
                        "parts": [
                          { "text": "ok" }
                        ]
                      }
                    }
                  ]
                }
                """, Encoding.UTF8, "application/json")
            };
        }
    }

    private sealed class CountingHandlerForOllama(HttpStatusCode statusCode, string body, int failCount) : HttpMessageHandler
    {
        public int Attempts { get; private set; }
        private readonly string _body = body;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Attempts++;
            if (Attempts <= failCount)
            {
                await Task.Delay(1, cancellationToken);
                return new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                {
                  "model": "qwen2.5:7b",
                  "message": {
                    "role": "assistant",
                    "content": "ok"
                  }
                }
                """, Encoding.UTF8, "application/json")
            };
        }
    }
}
