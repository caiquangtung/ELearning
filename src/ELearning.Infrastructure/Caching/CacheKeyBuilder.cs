using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ELearning.Core.Abstractions;

namespace ELearning.Infrastructure.Caching;

public sealed class CacheKeyBuilder : ICacheKeyBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public string Build(params string[] parts)
        => string.Join(':', parts.Select(Normalize).Where(p => p.Length > 0));

    public string BuildHashKey(string prefix, object value)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        var hash = Convert.ToHexString(bytes).ToLowerInvariant()[..24];
        return Build(prefix, hash);
    }

    private static string Normalize(string value)
        => value.Trim().ToLowerInvariant().Replace(' ', '-');
}
