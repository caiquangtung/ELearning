using System.Text.RegularExpressions;

namespace ELearning.Application.Common;

public static partial class SlugHelper
{
    public static string Slugify(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = NonAlphanumeric().Replace(s, "-");
        s = DuplicateHyphens().Replace(s, "-").Trim('-');
        return string.IsNullOrEmpty(s) ? "org" : s;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-+", RegexOptions.Compiled)]
    private static partial Regex DuplicateHyphens();
}
