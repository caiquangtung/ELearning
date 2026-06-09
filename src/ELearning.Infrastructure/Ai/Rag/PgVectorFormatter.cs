using System.Globalization;
using System.Text.Json;

namespace ELearning.Infrastructure.Ai;

internal static class PgVectorFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static string ToVectorLiteral(IReadOnlyList<float> vector) =>
        "[" + string.Join(",", vector.Select(x => x.ToString("0.########", CultureInfo.InvariantCulture))) + "]";

    public static string ToJson(IReadOnlyList<float> vector) =>
        JsonSerializer.Serialize(vector, JsonOptions);
}
