using System.Text.RegularExpressions;
using ELearning.Domain.Aggregates.CourseAggregate;

namespace ELearning.Infrastructure.Ai;

public sealed partial class AiKnowledgeChunker
{
    public const int DefaultMaxChunkCharacters = 1200;
    private const int MinimumChunkCharacters = 80;

    public IReadOnlyList<AiKnowledgeChunkSource> BuildCourseChunks(Course course, int maxChunkCharacters = DefaultMaxChunkCharacters)
    {
        var chunks = new List<AiKnowledgeChunkSource>();
        var maxLength = Math.Clamp(maxChunkCharacters, 400, 2500);

        var overview = Normalize($"{course.Title}\n{course.Description}");
        if (overview.Length >= MinimumChunkCharacters)
        {
            chunks.Add(new AiKnowledgeChunkSource(
                course.Id,
                null,
                null,
                "Course",
                course.Title,
                null,
                null,
                0,
                overview));
        }

        foreach (var section in course.Sections.OrderBy(x => x.SortOrder))
        {
            foreach (var lesson in section.Lessons.OrderBy(x => x.SortOrder))
            {
                var text = Normalize($"""
                    Course: {course.Title}
                    Section: {section.Title}
                    Lesson: {lesson.Title}

                    {lesson.Content}
                    """);

                foreach (var (chunkText, index) in SplitText(text, maxLength).Select((value, index) => (value, index)))
                {
                    chunks.Add(new AiKnowledgeChunkSource(
                        course.Id,
                        section.Id,
                        lesson.Id,
                        "Lesson",
                        course.Title,
                        section.Title,
                        lesson.Title,
                        index,
                        chunkText));
                }
            }
        }

        return chunks;
    }

    internal static IReadOnlyList<string> SplitText(string text, int maxCharacters)
    {
        var normalized = Normalize(text);
        if (normalized.Length == 0)
            return [];
        if (normalized.Length <= maxCharacters)
            return [normalized];

        var sentences = SentenceRegex()
            .Split(normalized)
            .Select(x => x.Trim())
            .Where(x => x.Length > 0)
            .ToList();

        if (sentences.Count == 0)
            sentences.Add(normalized);

        var chunks = new List<string>();
        var current = "";

        foreach (var sentence in sentences)
        {
            if (sentence.Length > maxCharacters)
            {
                Flush();
                for (var offset = 0; offset < sentence.Length; offset += maxCharacters)
                    chunks.Add(sentence.Substring(offset, Math.Min(maxCharacters, sentence.Length - offset)).Trim());
                continue;
            }

            var candidate = string.IsNullOrWhiteSpace(current) ? sentence : $"{current} {sentence}";
            if (candidate.Length > maxCharacters)
            {
                Flush();
                current = sentence;
            }
            else
            {
                current = candidate;
            }
        }

        Flush();
        return chunks.Where(x => x.Length >= MinimumChunkCharacters).ToList();

        void Flush()
        {
            if (!string.IsNullOrWhiteSpace(current))
            {
                chunks.Add(current.Trim());
                current = "";
            }
        }
    }

    private static string Normalize(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? ""
            : WhitespaceRegex().Replace(text.Trim(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"(?<=[.!?])\s+")]
    private static partial Regex SentenceRegex();
}

public sealed record AiKnowledgeChunkSource(
    Guid CourseId,
    Guid? SectionId,
    Guid? LessonId,
    string SourceType,
    string CourseTitle,
    string? SectionTitle,
    string? LessonTitle,
    int ChunkIndex,
    string Text);
