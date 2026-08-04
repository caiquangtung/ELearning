using System.Net;
using System.Text;
using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ELearning.Application.UnitTests;

public class OpenAiCompatibleAiProviderTests
{
    [Fact]
    public async Task Quiz_provider_parses_structured_questions()
    {
        var provider = CreateQuizProvider(ChatResponse(
            """
            {"questions":[{"text":"What is JWT used for?","type":"MultipleChoice","points":1,"difficulty":"Medium","explanation":"JWT carries signed claims.","options":[{"text":"Signed claims","isCorrect":true,"sortOrder":1},{"text":"Database migration","isCorrect":false,"sortOrder":2}]}]}
            """));

        var result = await provider.GenerateAsync(QuizRequest());

        result.Provider.Should().Be("OpenAiCompatible");
        result.Model.Should().Be("gpt-test");
        result.TokenEstimate.Should().Be(42);
        result.Questions.Should().ContainSingle();
        result.Questions[0].Options.Should().ContainSingle(x => x.IsCorrect);
    }

    [Fact]
    public async Task Quiz_provider_rejects_invalid_multiple_choice_output()
    {
        var provider = CreateQuizProvider(ChatResponse(
            """
            {"questions":[{"text":"What is JWT used for?","type":"MultipleChoice","points":1,"difficulty":"Medium","explanation":"Bad output.","options":[{"text":"A","isCorrect":true,"sortOrder":1},{"text":"B","isCorrect":true,"sortOrder":2}]}]}
            """));

        var act = () => provider.GenerateAsync(QuizRequest());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exactly one correct option*");
    }

    [Fact]
    public async Task Essay_provider_rejects_score_outside_question_bounds()
    {
        var questionId = Guid.NewGuid();
        var provider = CreateEssayProvider(ChatResponse(
            $$"""
            {"suggestions":[{"questionId":"{{questionId}}","suggestedScore":8,"confidence":0.7,"reasoning":"Too high.","rubricBreakdown":[]}]}
            """));

        var request = new AiEssayGradingRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Security quiz",
            [new AiEssayAnswerInput(questionId, "Explain JWT validation.", "It validates signed claims.", 5)],
            null);

        var act = () => provider.SuggestAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*outside the question bounds*");
    }

    [Fact]
    public void Learning_path_mapper_drops_hallucinated_course_ids()
    {
        var realCourse = Course.Create("API Security", "JWT, authorization, and audit logging.");
        var section = realCourse.AddSection("Authentication");
        section.AddLesson("JWT validation");
        realCourse.Publish();

        var hallucinatedCourseId = Guid.NewGuid();
        var providerJson =
            $$"""
            {"confidence":0.82,"estimatedEffort":"2-4 weeks","missingSkills":["OAuth"],"courses":[{"courseId":"{{hallucinatedCourseId}}","score":91,"estimatedEffort":"1 week","reasons":["Looks plausible"]},{"courseId":"{{realCourse.Id}}","score":88,"estimatedEffort":"2 weeks","reasons":["Matches API security"]}]}
            """;

        var mapped = OpenAiCompatibleLearningPathService.BuildPathCoursesFromProviderJson(
            providerJson,
            [realCourse],
            5);

        mapped.Should().ContainSingle();
        mapped[0].CourseId.Should().Be(realCourse.Id);
        mapped[0].Score.Should().Be(88);
    }

    [Fact]
    public async Task Di_registration_selects_openai_compatible_provider_when_configured()
    {
        using var provider = BuildQuizServiceProvider(
            new AiOptions
            {
                Provider = "OpenAiCompatible",
                ApiKey = "test-key",
                ChatModel = "gpt-test",
                FallbackToLocal = false
            },
            ChatResponse(
                """
                {"questions":[{"text":"What is authorization?","type":"MultipleChoice","points":1,"difficulty":"Medium","explanation":"Authorization controls access.","options":[{"text":"Access control","isCorrect":true,"sortOrder":1},{"text":"Styling","isCorrect":false,"sortOrder":2}]}]}
                """));

        var service = provider.GetRequiredService<IAiQuizQuestionGenerator>();
        var result = await service.GenerateAsync(QuizRequest());

        result.Provider.Should().Be("OpenAiCompatible");
    }

    [Fact]
    public async Task Configurable_provider_falls_back_to_local_when_real_provider_fails()
    {
        using var provider = BuildQuizServiceProvider(
            new AiOptions
            {
                Provider = "OpenAiCompatible",
                ApiKey = "",
                ChatModel = "gpt-test",
                FallbackToLocal = true
            },
            ChatResponse("{}"));

        var service = provider.GetRequiredService<IAiQuizQuestionGenerator>();
        var result = await service.GenerateAsync(QuizRequest());

        result.Provider.Should().Be("Local");
        result.Questions.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Google_ai_studio_chat_client_parses_multi_part_candidate_content()
    {
        var options = Options.Create(new AiOptions
        {
            RagChatProvider = "GoogleAiStudio",
            RagChatModel = "gemini-2.5-flash",
            RagEmbeddingApiKey = "test-key",
            RagEmbeddingBaseUrl = "https://generativelanguage.googleapis.com/v1beta"
        });

        var response = JsonSerializer.Serialize(new
        {
            modelVersion = "gemini-test",
            candidates = new[]
            {
                new
                {
                    content = new
                    {
                        parts = new object[]
                        {
                            new { text = "Hello" },
                            new { text = ", world" }
                        }
                    }
                }
            },
            usageMetadata = new { totalTokenCount = 12 }
        });

        var client = new GoogleAiStudioChatClient(new HttpClient(new FakeHttpMessageHandler(response)), options);

        var result = await client.CompleteJsonAsync("system", "user");

        result.Provider.Should().Be("GoogleAiStudio");
        result.Model.Should().Be("gemini-test");
        result.Content.Should().Be("Hello, world");
        result.TokenEstimate.Should().Be(12);
    }

    [Fact]
    public async Task Google_ai_studio_chat_client_rejects_empty_candidates()
    {
        var options = Options.Create(new AiOptions
        {
            RagChatProvider = "GoogleAiStudio",
            RagChatModel = "gemini-2.5-flash",
            RagEmbeddingApiKey = "test-key",
            RagEmbeddingBaseUrl = "https://generativelanguage.googleapis.com/v1beta"
        });

        var response = JsonSerializer.Serialize(new
        {
            candidates = Array.Empty<object>()
        });

        var client = new GoogleAiStudioChatClient(new HttpClient(new FakeHttpMessageHandler(response)), options);

        var act = () => client.CompleteJsonAsync("system", "user");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*did not include any candidates*");
    }

    private static ServiceProvider BuildQuizServiceProvider(AiOptions options, string responseBody)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Options.Create(options));
        services.AddSingleton(new HttpClient(new FakeHttpMessageHandler(responseBody)));
        services.AddScoped<OpenAiCompatibleChatClient>();
        services.AddSingleton<LocalQuizQuestionGenerator>();
        services.AddScoped<OpenAiCompatibleQuizQuestionGenerator>();
        services.AddScoped<IAiQuizQuestionGenerator, ConfigurableAiQuizQuestionGenerator>();
        return services.BuildServiceProvider();
    }

    private static OpenAiCompatibleQuizQuestionGenerator CreateQuizProvider(string responseBody)
    {
        var options = Options.Create(new AiOptions
        {
            Provider = "OpenAiCompatible",
            ApiKey = "test-key",
            ChatModel = "gpt-test",
            FallbackToLocal = false
        });

        var client = new OpenAiCompatibleChatClient(new HttpClient(new FakeHttpMessageHandler(responseBody)), options);
        return new OpenAiCompatibleQuizQuestionGenerator(client, options);
    }

    private static OpenAiCompatibleEssayGradingService CreateEssayProvider(string responseBody)
    {
        var options = Options.Create(new AiOptions
        {
            Provider = "OpenAiCompatible",
            ApiKey = "test-key",
            ChatModel = "gpt-test",
            FallbackToLocal = false
        });

        var client = new OpenAiCompatibleChatClient(new HttpClient(new FakeHttpMessageHandler(responseBody)), options);
        return new OpenAiCompatibleEssayGradingService(client, options);
    }

    private static AiQuizQuestionGenerationRequest QuizRequest() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Secure API Development",
            "Authentication authorization validation and audit logging.",
            "JWT Authentication",
            "JWT authentication validates signed tokens and maps claims to permissions.",
            1,
            "Medium",
            ["MultipleChoice"]);

    private static string ChatResponse(string content)
    {
        var response = new
        {
            model = "gpt-test",
            choices = new[] { new { message = new { content } } },
            usage = new { total_tokens = 42 }
        };

        return JsonSerializer.Serialize(response);
    }

    private sealed class FakeHttpMessageHandler(string responseBody) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization?.Scheme.Should().Be("Bearer");

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
            });
        }
    }
}
