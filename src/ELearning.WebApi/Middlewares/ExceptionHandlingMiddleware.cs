using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace ELearning.WebApi.Middlewares;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            if (context.Response.HasStarted)
                throw;

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            var payload = new
            {
                title = "Server Error",
                status = context.Response.StatusCode,
                detail = context.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment()
                    ? ex.ToString()
                    : "An unexpected error occurred."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
