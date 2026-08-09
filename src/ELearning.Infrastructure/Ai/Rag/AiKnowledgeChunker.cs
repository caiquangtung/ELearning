using System.Text;
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

        var structureBuilder = new StringBuilder();
        structureBuilder.AppendLine($"Course: {course.Title} Structure and Table of Contents.");
        structureBuilder.AppendLine($"Description: {course.Description}");
        
        var totalLessons = 0;
        var sectionIndex = 1;
        foreach (var section in course.Sections.OrderBy(x => x.SortOrder))
        {
            structureBuilder.AppendLine($"- Section {sectionIndex}: {section.Title}");
            var lessonIndex = 1;
            foreach (var lesson in section.Lessons.OrderBy(x => x.SortOrder))
            {
                structureBuilder.AppendLine($"  * Lesson {lessonIndex}: {lesson.Title}");
                lessonIndex++;
                totalLessons++;
            }
            sectionIndex++;
        }
        structureBuilder.AppendLine($"Summary: This course contains {course.Sections.Count} sections and {totalLessons} lessons.");

        var structureText = Normalize(structureBuilder.ToString());
        if (structureText.Length >= MinimumChunkCharacters)
        {
            chunks.Add(new AiKnowledgeChunkSource(
                course.Id,
                null,
                null,
                "Structure",
                course.Title,
                null,
                null,
                0,
                structureText));
        }

        foreach (var section in course.Sections.OrderBy(x => x.SortOrder))
        {
            foreach (var lesson in section.Lessons.OrderBy(x => x.SortOrder))
            {
                var contextHeader = $"[Context: Course '{course.Title}' > Section '{section.Title}' > Lesson '{lesson.Title}']";
                var lessonText = Normalize(lesson.Content);
                if (lessonText.Length == 0)
                    continue;

                var subChunks = SplitText(lessonText, Math.Max(200, maxLength - contextHeader.Length - 10));
                foreach (var (chunkText, index) in subChunks.Select((value, index) => (value, index)))
                {
                    var contextualText = Normalize($"{contextHeader}\n{chunkText}");
                    chunks.Add(new AiKnowledgeChunkSource(
                        course.Id,
                        section.Id,
                        lesson.Id,
                        "Lesson",
                        course.Title,
                        section.Title,
                        lesson.Title,
                        index,
                        contextualText));
                }
            }
        }

        return chunks;
    }

    internal static IReadOnlyList<string> SplitText(string text, int maxCharacters, int overlapCharacters = 180)
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
        var currentSentences = new List<string>();
        var currentLength = 0;

        foreach (var sentence in sentences)
        {
            if (sentence.Length > maxCharacters)
            {
                FlushCurrent();
                for (var offset = 0; offset < sentence.Length; offset += maxCharacters)
                {
                    var piece = sentence.Substring(offset, Math.Min(maxCharacters, sentence.Length - offset)).Trim();
                    if (piece.Length >= MinimumChunkCharacters)
                        chunks.Add(piece);
                }
                currentSentences.Clear();
                currentLength = 0;
                continue;
            }

            var addedLength = currentLength == 0 ? sentence.Length : sentence.Length + 1;
            if (currentLength + addedLength > maxCharacters && currentSentences.Count > 0)
            {
                var previousSentences = currentSentences.ToList();
                FlushCurrent();

                var overlapSentences = new List<string>();
                var accumulatedOverlapLength = 0;
                for (int i = previousSentences.Count - 1; i >= 0; i--)
                {
                    var s = previousSentences[i];
                    var len = accumulatedOverlapLength == 0 ? s.Length : s.Length + 1;
                    if (accumulatedOverlapLength + len <= overlapCharacters)
                    {
                        overlapSentences.Insert(0, s);
                        accumulatedOverlapLength += len;
                    }
                    else
                    {
                        break;
                    }
                }

                currentSentences = overlapSentences;
                currentLength = accumulatedOverlapLength;
            }

            currentSentences.Add(sentence);
            currentLength += currentLength == 0 ? sentence.Length : sentence.Length + 1;
        }

        FlushCurrent();
        return chunks.Where(x => x.Length >= MinimumChunkCharacters).ToList();

        void FlushCurrent()
        {
            if (currentSentences.Count > 0)
            {
                var chunkText = string.Join(" ", currentSentences).Trim();
                if (!string.IsNullOrWhiteSpace(chunkText))
                {
                    chunks.Add(chunkText);
                }
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
