using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed partial class LocalQuizQuestionGenerator(IOptions<AiOptions> options)
    : IAiQuizQuestionGenerator
{
    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "about", "after", "again", "also", "because", "before", "between", "course", "could", "create",
        "describe", "during", "example", "first", "from", "have", "into", "lesson", "more", "should",
        "their", "there", "these", "thing", "this", "through", "using", "when", "where", "which", "while",
        "with", "without", "would", "your"
    };

    public Task<AiQuizQuestionGenerationResult> GenerateAsync(
        AiQuizQuestionGenerationRequest request,
        CancellationToken ct = default)
    {
        var config = options.Value;
        var source = BuildSource(request, config.MaxSourceCharacters);
        var keywords = ExtractKeywords(source).DefaultIfEmpty(request.CourseTitle).Take(Math.Max(4, request.QuestionCount)).ToList();
        var types = request.QuestionTypes.Count == 0 ? ["MultipleChoice"] : request.QuestionTypes;
        var questions = new List<AiGeneratedQuestion>();

        for (var i = 0; i < request.QuestionCount; i++)
        {
            ct.ThrowIfCancellationRequested();

            var type = NormalizeType(types[i % types.Count]);
            var keyword = keywords[i % keywords.Count];
            questions.Add(type switch
            {
                "Essay" => CreateEssayQuestion(request, keyword, i + 1),
                "Code" => CreateCodeQuestion(request, keyword, i + 1),
                _ => CreateMultipleChoiceQuestion(request, keyword, keywords, i + 1)
            });
        }

        var tokenEstimate = Math.Max(1, source.Length / 4);
        return Task.FromResult(new AiQuizQuestionGenerationResult(
            string.IsNullOrWhiteSpace(config.Provider) ? "Local" : config.Provider,
            string.IsNullOrWhiteSpace(config.Model) ? "local-deterministic-v1" : config.Model,
            string.IsNullOrWhiteSpace(config.QuizQuestionPromptVersion)
                ? "quiz-question-generator-v1"
                : config.QuizQuestionPromptVersion,
            tokenEstimate,
            questions));
    }

    private static AiGeneratedQuestion CreateMultipleChoiceQuestion(
        AiQuizQuestionGenerationRequest request,
        string keyword,
        IReadOnlyList<string> keywords,
        int sortOrder)
    {
        var context = request.LessonTitle ?? request.CourseTitle;
        var distractors = keywords
            .Where(k => !k.Equals(keyword, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .ToList();

        while (distractors.Count < 3)
            distractors.Add($"related concept {distractors.Count + 1}");

        return new AiGeneratedQuestion(
            $"Which statement best describes {keyword} in {context}?",
            "MultipleChoice",
            1,
            sortOrder,
            request.Difficulty,
            $"{keyword} is identified from the selected lesson/course content and should be reviewed by the instructor.",
            [
                new AiGeneratedQuestionOption($"{keyword} is a key concept discussed in {context}.", true, 1),
                new AiGeneratedQuestionOption($"{distractors[0]} is the only concept needed to complete the topic.", false, 2),
                new AiGeneratedQuestionOption($"{distractors[1]} replaces the need to understand {keyword}.", false, 3),
                new AiGeneratedQuestionOption($"{distractors[2]} is unrelated to the lesson outcome.", false, 4)
            ]);
    }

    private static AiGeneratedQuestion CreateEssayQuestion(
        AiQuizQuestionGenerationRequest request,
        string keyword,
        int sortOrder)
    {
        var context = request.LessonTitle ?? request.CourseTitle;
        return new AiGeneratedQuestion(
            $"Explain how {keyword} applies to {context}.",
            "Essay",
            request.Difficulty.Equals("Hard", StringComparison.OrdinalIgnoreCase) ? 8 : 5,
            sortOrder,
            request.Difficulty,
            "A strong answer should connect the concept to the lesson content and include a concrete example.",
            []);
    }

    private static AiGeneratedQuestion CreateCodeQuestion(
        AiQuizQuestionGenerationRequest request,
        string keyword,
        int sortOrder)
    {
        var context = request.LessonTitle ?? request.CourseTitle;
        return new AiGeneratedQuestion(
            $"Write pseudocode or a short implementation that demonstrates {keyword} from {context}.",
            "Code",
            request.Difficulty.Equals("Hard", StringComparison.OrdinalIgnoreCase) ? 10 : 6,
            sortOrder,
            request.Difficulty,
            "Evaluate whether the submitted code demonstrates the requested concept and handles the main scenario.",
            []);
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

    private static IReadOnlyList<string> ExtractKeywords(string source) =>
        WordRegex()
            .Matches(source)
            .Select(m => m.Value.Trim().ToLowerInvariant())
            .Where(w => w.Length >= 5 && !StopWords.Contains(w))
            .GroupBy(w => w)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key)
            .Select(g => char.ToUpperInvariant(g.Key[0]) + g.Key[1..])
            .Take(20)
            .ToList();

    private static string NormalizeType(string type) =>
        type.Trim().ToLowerInvariant() switch
        {
            "essay" => "Essay",
            "code" => "Code",
            _ => "MultipleChoice"
        };

    [GeneratedRegex("[A-Za-z][A-Za-z0-9_-]+")]
    private static partial Regex WordRegex();
}
