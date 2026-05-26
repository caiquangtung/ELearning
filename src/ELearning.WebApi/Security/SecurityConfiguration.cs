using ELearning.Application.Common.Options;

namespace ELearning.WebApi.Security;

public static class SecurityConfiguration
{
    public static string[] GetAllowedOrigins(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var configured = configuration["Cors:AllowedOrigins"];
        var origins = ParseAllowedOrigins(configured);

        if (origins.Length == 0 && environment.IsDevelopment())
            return ["http://localhost:4200"];

        if (origins.Length == 0)
            throw new InvalidOperationException("Cors:AllowedOrigins must be configured outside Development.");

        return origins;
    }

    public static string[] ParseAllowedOrigins(string? configured)
    {
        if (string.IsNullOrWhiteSpace(configured))
            return [];

        var origins = configured
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (origins.Any(o => o is "*" or "null"))
            throw new InvalidOperationException("Cors:AllowedOrigins cannot contain wildcard or null origins.");

        foreach (var origin in origins)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                uri.Scheme is not ("http" or "https") ||
                !string.IsNullOrWhiteSpace(uri.AbsolutePath.Trim('/')) ||
                !string.IsNullOrWhiteSpace(uri.Query) ||
                !string.IsNullOrWhiteSpace(uri.Fragment))
            {
                throw new InvalidOperationException($"Cors:AllowedOrigins contains an invalid origin: {origin}");
            }
        }

        return origins;
    }

    public static void ValidateProductionSecrets(IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
            return;

        var jwtSecret = configuration["JwtSettings:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
            throw new InvalidOperationException("JwtSettings:Secret must be at least 32 characters outside Development.");

        var webhookSecret = configuration[$"{PaymentOptions.SectionName}:WebhookSecret"];
        if (string.IsNullOrWhiteSpace(webhookSecret))
            throw new InvalidOperationException("Payments:WebhookSecret must be configured outside Development.");
    }
}
