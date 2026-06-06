namespace ELearning.Infrastructure.Ai;

internal static class OpenAiCompatibleJson
{
    public static string ExtractObject(string content)
    {
        var value = content.Trim();
        if (value.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = value.IndexOf('\n', StringComparison.Ordinal);
            if (firstLineEnd >= 0)
                value = value[(firstLineEnd + 1)..];

            var fenceIndex = value.LastIndexOf("```", StringComparison.Ordinal);
            if (fenceIndex >= 0)
                value = value[..fenceIndex];

            value = value.Trim();
        }

        var start = value.IndexOf('{');
        var end = value.LastIndexOf('}');
        if (start < 0 || end < start)
            throw new InvalidOperationException("AI provider did not return a JSON object.");

        return value[start..(end + 1)];
    }

    public static int EstimateTokens(params string?[] values)
    {
        var length = values.Where(x => !string.IsNullOrWhiteSpace(x)).Sum(x => x!.Length);
        return Math.Max(1, (int)Math.Ceiling(length / 4m));
    }
}
