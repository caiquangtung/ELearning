using System.Security.Cryptography;
using System.Text;

namespace ELearning.Core.Common;

public static class RefreshTokenHasher
{
    public static string Hash(string rawRefreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public static string GenerateRawToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
