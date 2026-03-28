using ELearning.Domain.Aggregates.UserAggregate;
using System.Security.Claims;

namespace ELearning.Core.Abstractions;

public record TokenPair(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAt);

public interface ITokenService
{
    TokenPair GenerateTokenPair(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
    string HashToken(string token);
}
