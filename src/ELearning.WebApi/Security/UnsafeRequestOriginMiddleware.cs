using Microsoft.Extensions.Options;

namespace ELearning.WebApi.Security;

public sealed class UnsafeRequestOriginMiddleware(
    RequestDelegate next,
    IOptions<SecurityOptions> options,
    IConfiguration configuration,
    IWebHostEnvironment environment)
{
    private readonly SecurityOptions _options = options.Value;
    private readonly HashSet<string> _allowedOrigins = SecurityConfiguration
        .GetAllowedOrigins(configuration, environment)
        .Select(NormalizeOrigin)
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.ValidateUnsafeRequestOrigins || IsSafeMethod(context.Request.Method))
        {
            await next(context);
            return;
        }

        var origin = ResolveRequestOrigin(context.Request);
        if (origin is null || _allowedOrigins.Contains(origin) || IsSelfOrigin(context.Request, origin))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { message = "Request origin is not allowed." });
    }

    public static string? ResolveRequestOrigin(HttpRequest request)
    {
        var origin = request.Headers.Origin.ToString();
        if (!string.IsNullOrWhiteSpace(origin) && Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
            return NormalizeOrigin(originUri);

        var referer = request.Headers.Referer.ToString();
        if (!string.IsNullOrWhiteSpace(referer) && Uri.TryCreate(referer, UriKind.Absolute, out var refererUri))
            return NormalizeOrigin(refererUri);

        return null;
    }

    private static bool IsSafeMethod(string method) =>
        HttpMethods.IsGet(method) ||
        HttpMethods.IsHead(method) ||
        HttpMethods.IsOptions(method) ||
        HttpMethods.IsTrace(method);

    private static string NormalizeOrigin(Uri uri) =>
        uri.IsDefaultPort ? $"{uri.Scheme}://{uri.Host}" : $"{uri.Scheme}://{uri.Host}:{uri.Port}";

    private static string NormalizeOrigin(string origin) =>
        Uri.TryCreate(origin, UriKind.Absolute, out var uri) ? NormalizeOrigin(uri) : origin.TrimEnd('/');

    private static bool IsSelfOrigin(HttpRequest request, string origin)
    {
        var self = request.Host.Port is null
            ? $"{request.Scheme}://{request.Host.Host}"
            : $"{request.Scheme}://{request.Host.Host}:{request.Host.Port}";

        return string.Equals(origin, self, StringComparison.OrdinalIgnoreCase);
    }
}
