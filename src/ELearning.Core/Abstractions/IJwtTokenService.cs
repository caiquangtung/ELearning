namespace ELearning.Core.Abstractions;

public record AuthTokenPair(
    string AccessToken,
    string RawRefreshToken,
    DateTime AccessTokenExpiresAtUtc,
    DateTime RefreshTokenExpiresAtUtc);

public interface IJwtTokenService
{
    AuthTokenPair CreateTokenPair(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> roles);
}
