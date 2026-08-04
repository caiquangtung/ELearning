using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace ELearning.Infrastructure.Ai;

internal static class PromptTemplateStore
{
    private static readonly ConcurrentDictionary<string, string> Cache = new(StringComparer.OrdinalIgnoreCase);

    internal static string LoadSystemPrompt(string promptVersion, string fallbackPrompt) =>
        LoadPrompt(promptVersion, "system.md", fallbackPrompt);

    internal static string LoadPrompt(string promptVersion, string fileSuffix, string fallbackPrompt)
    {
        if (string.IsNullOrWhiteSpace(promptVersion))
            return Normalize(fallbackPrompt);

        return Cache.GetOrAdd(
            $"{promptVersion}:{fileSuffix}",
            _ => LoadCore(promptVersion.Trim(), fileSuffix.Trim(), fallbackPrompt));
    }

    private static string LoadCore(string promptVersion, string fileSuffix, string fallbackPrompt)
    {
        var fileName = $"{promptVersion}.{fileSuffix}";
        var filePath = Path.Combine(AppContext.BaseDirectory, "Ai", "Prompts", fileName);
        if (File.Exists(filePath))
            return Normalize(File.ReadAllText(filePath, Encoding.UTF8));

        var assembly = typeof(PromptTemplateStore).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith($"Ai.Prompts.{fileName}", StringComparison.OrdinalIgnoreCase));

        if (resourceName is not null)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is not null)
            {
                using var reader = new StreamReader(stream, Encoding.UTF8, true);
                return Normalize(reader.ReadToEnd());
            }
        }

        return Normalize(fallbackPrompt);
    }

    private static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}