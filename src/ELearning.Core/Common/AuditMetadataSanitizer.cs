namespace ELearning.Core.Common;

public static class AuditMetadataSanitizer
{
    private static readonly string[] SensitiveFragments =
    [
        "password",
        "secret",
        "token",
        "authorization",
        "credential",
        "apikey",
        "api_key"
    ];

    public static IReadOnlyDictionary<string, string> Sanitize(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
            return new Dictionary<string, string>();

        return metadata.ToDictionary(
            pair => pair.Key,
            pair => IsSensitiveKey(pair.Key) ? "[redacted]" : pair.Value);
    }

    private static bool IsSensitiveKey(string key) =>
        SensitiveFragments.Any(fragment => key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}
