using Microsoft.Extensions.Options;

namespace ELearning.WebApi.Security;

public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IOptions<SecurityOptions> options,
    IWebHostEnvironment environment)
{
    private readonly SecurityOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        if (_options.EnableSecurityHeaders)
        {
            var headers = context.Response.Headers;
            headers.TryAdd("X-Content-Type-Options", "nosniff");
            headers.TryAdd("X-Frame-Options", "DENY");
            headers.TryAdd("Referrer-Policy", _options.ReferrerPolicy);
            headers.TryAdd("Permissions-Policy", _options.PermissionsPolicy);

            if (!string.IsNullOrWhiteSpace(_options.ContentSecurityPolicy))
                headers.TryAdd("Content-Security-Policy", _options.ContentSecurityPolicy);

            if (!environment.IsDevelopment() && _options.HstsMaxAgeDays > 0)
                headers.TryAdd("Strict-Transport-Security", $"max-age={_options.HstsMaxAgeDays * 86400}; includeSubDomains");
        }

        await next(context);
    }
}
