using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed class OpenAiCompatibleQuizQuestionGenerator(
    OpenAiCompatibleChatClient client,
    IOptions<AiOptions> options)
    : IAiQuizQuestionGenerator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AiQuizQuestionGenerationResult> GenerateAsync(
        AiQuizQuestionGenerationRequest request,
        CancellationToken ct = default)
    {
        var config = options.Value;
        var source = BuildSource(request, config.MaxSourceCharacters);
        var result = await client.CompleteJsonAsync(
            BuildSystemPrompt(),
            BuildUserPrompt(request, source),
            ct);

        var response = JsonSerializer.Deserialize<QuizQuestionResponse>(
            OpenAiCompatibleJson.ExtractObject(result.Content),
            JsonOptions);

        if (response?.Questions is null || response.Questions.Count == 0)
            throw new InvalidOperationException("AI provider returned no quiz questions.");

        var questions = response.Questions
            .Take(request.QuestionCount)
            .Select((question, index) => ToQuestion(question, request, index + 1))
            .ToList();

        return new AiQuizQuestionGenerationResult(
            result.Provider,
            result.Model,
            string.IsNullOrWhiteSpace(config.QuizQuestionPromptVersion)
                ? "quiz-question-generator-v1"
                : config.QuizQuestionPromptVersion,
            result.TokenEstimate ?? OpenAiCompatibleJson.EstimateTokens(source, result.Content),
            questions);
    }

    private static AiGeneratedQuestion ToQuestion(
        QuizQuestionItem item,
        AiQuizQuestionGenerationRequest request,
        int sortOrder)
    {
        var type = NormalizeType(item.Type);
        var options = (item.Options ?? [])
            .Where(option => !string.IsNullOrWhiteSpace(option.Text))
            .Select((option, index) => new AiGeneratedQuestionOption(
                option.Text!.Trim(),
                option.IsCorrect,
                option.SortOrder <= 0 ? index + 1 : option.SortOrder))
            .OrderBy(option => option.SortOrder)
            .ToList();

        if (type == "MultipleChoice")
        {
            if (options.Count < 2)
                throw new InvalidOperationException("AI provider returned a multiple-choice question without enough options.");
            if (options.Count(option => option.IsCorrect) != 1)
                throw new InvalidOperationException("AI provider returned a multiple-choice question without exactly one correct option.");
        }
        else
        {
            options = [];
        }

        if (string.IsNullOrWhiteSpace(item.Text))
            throw new InvalidOperationException("AI provider returned a question without text.");

        return new AiGeneratedQuestion(
            item.Text.Trim(),
            type,
            Math.Clamp(item.Points <= 0 ? DefaultPoints(type, request.Difficulty) : item.Points, 1, 20),
            sortOrder,
            NormalizeDifficulty(string.IsNullOrWhiteSpace(item.Difficulty) ? request.Difficulty : item.Difficulty),
            string.IsNullOrWhiteSpace(item.Explanation)
                ? "Review this generated question before adding it to the quiz."
                : item.Explanation.Trim(),
            options);
    }

    private static string BuildSystemPrompt() =>
        PromptTemplateStore.LoadSystemPrompt(
            "quiz-question-generator-v1",
            """
            You generate LMS quiz question drafts for instructors. Return only a JSON object.
            The JSON shape must be:
            {"questions":[{"text":"...","type":"MultipleChoice|Essay|Code","points":1,"difficulty":"Easy|Medium|Hard","explanation":"...","options":[{"text":"...","isCorrect":true,"sortOrder":1}]}]}
            MultipleChoice questions must have exactly one correct option. Essay and Code questions must have an empty options array.
            """);

    private static string BuildUserPrompt(AiQuizQuestionGenerationRequest request, string source)
    {
        var payload = new
        {
            request.CourseId,
            request.LessonId,
            request.CourseTitle,
            request.LessonTitle,
            request.QuestionCount,
            Difficulty = NormalizeDifficulty(request.Difficulty),
            QuestionTypes = request.QuestionTypes.Select(NormalizeType).ToArray(),
            Source = source
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static string BuildSource(AiQuizQuestionGenerationRequest request, int maxCharacters)
    {
        var source = string.Join(
            Environment.NewLine,
            request.CourseTitle,
            request.CourseDescription,
            request.LessonTitle,
            request.LessonContent);

        return source.Length <= maxCharacters ? source : source[..maxCharacters];
    }

    private static string NormalizeType(string? type) =>
        type?.Trim().ToLowerInvariant() switch
        {
            "essay" => "Essay",
            "code" => "Code",
            _ => "MultipleChoice"
        };

    private static string NormalizeDifficulty(string? difficulty) =>
        difficulty?.Trim().ToLowerInvariant() switch
        {
            "easy" => "Easy",
            "hard" => "Hard",
            _ => "Medium"
        };

    private static int DefaultPoints(string type, string difficulty) =>
        type switch
        {
            "Code" => NormalizeDifficulty(difficulty) == "Hard" ? 10 : 6,
            "Essay" => NormalizeDifficulty(difficulty) == "Hard" ? 8 : 5,
            _ => 1
        };

    private sealed record QuizQuestionResponse(IReadOnlyList<QuizQuestionItem> Questions);

    private sealed record QuizQuestionItem(
        string? Text,
        string? Type,
        int Points,
        string? Difficulty,
        string? Explanation,
        IReadOnlyList<QuizQuestionOptionItem>? Options);

    private sealed record QuizQuestionOptionItem(string? Text, bool IsCorrect, int SortOrder);
}
