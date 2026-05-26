using System.Security.Cryptography;
using System.Text;

namespace ELearning.WebApi.Security;

public static class WebhookSecretValidator
{
    public static bool IsValid(string? expected, string? actual)
    {
        if (string.IsNullOrEmpty(expected))
            return true;

        if (string.IsNullOrEmpty(actual))
            return false;

        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return expectedBytes.Length == actualBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
