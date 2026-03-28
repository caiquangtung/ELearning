using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ELearning.Infrastructure.Identity;

public sealed class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);
    private readonly JwtSettings _settings = options.Value;

    public AuthTokenPair CreateTokenPair(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> roles)
    {
        var now = DateTime.UtcNow;
        var accessExpires = now.AddMinutes(_settings.ExpiryMinutes);
        var refreshExpires = now.Add(RefreshTokenLifetime);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.GivenName, firstName),
            new(ClaimTypes.Surname, lastName)
        };
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessExpires,
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var rawRefresh = RefreshTokenHasher.GenerateRawToken();

        return new AuthTokenPair(accessToken, rawRefresh, accessExpires, refreshExpires);
    }
}
