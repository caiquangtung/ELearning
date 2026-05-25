using ELearning.Core.Abstractions;
using Microsoft.AspNetCore.Http.Extensions;

namespace ELearning.WebApi.Middlewares;

public sealed class RedisRateLimitingMiddleware(
    RequestDelegate next,
    IRateLimitStore rateLimitStore,
    ICacheKeyBuilder cacheKeyBuilder,
    ILogger<RedisRateLimitingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var policy = ResolvePolicy(context);
        if (policy is null)
        {
            await next(context);
            return;
        }

        var identity = context.User.Identity?.IsAuthenticated == true
            ? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("sub")?.Value
                ?? "anonymous"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var key = cacheKeyBuilder.Build("rate", policy.Value.Name, identity);
        var result = await rateLimitStore.IncrementAsync(key, policy.Value.Limit, policy.Value.Window, context.RequestAborted);

        if (!result.IsStoreAvailable)
            logger.LogWarning("Rate limit store unavailable for {Url}: {Reason}", context.Request.GetDisplayUrl(), result.FailureReason);

        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, result.Limit - result.Count).ToString();
        context.Response.Headers["X-RateLimit-Reset"] = result.ResetAt.ToUnixTimeSeconds().ToString();

        if (!result.IsAllowed)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsJsonAsync(new
            {
                message = "Too many requests. Please retry later.",
                retryAfterUtc = result.ResetAt
            }, context.RequestAborted);
            return;
        }

        await next(context);
    }

    private static RateLimitPolicy? ResolvePolicy(HttpContext context)
    {
        if (!HttpMethods.IsPost(context.Request.Method))
            return null;

        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        if (path.Contains("/identity/login") || path.Contains("/identity/register"))
            return new RateLimitPolicy("auth", 10, TimeSpan.FromMinutes(1));

        if (path.Contains("/payments/webhook"))
            return new RateLimitPolicy("webhook", 120, TimeSpan.FromMinutes(1));

        if (path.Contains("/checkout/quote") || path.Contains("/orders"))
            return new RateLimitPolicy("checkout", 30, TimeSpan.FromMinutes(1));

        if (path.Contains("/videos/") || path.Contains("/assets"))
            return new RateLimitPolicy("upload", 20, TimeSpan.FromMinutes(5));

        return null;
    }

    private readonly record struct RateLimitPolicy(string Name, int Limit, TimeSpan Window);
}
