namespace ELearning.WebApi.Middlewares;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        var id = context.Request.Headers[HeaderName].FirstOrDefault()
                 ?? Guid.NewGuid().ToString("D");
        context.Response.Headers[HeaderName] = id;
        context.Items["CorrelationId"] = id;
        context.Request.Headers[HeaderName] = id;
        await next(context);
    }
}
