using System.Net;
using System.Text.Json;
using ELearning.Core.Exceptions;

namespace ELearning.WebApi.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            ValidationException ve => (
                HttpStatusCode.UnprocessableEntity,
                "Validation Failed",
                (object)ve.Errors),

            NotFoundException nfe => (
                HttpStatusCode.NotFound,
                "Not Found",
                (object)new { message = nfe.Message }),

            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                (object)new { message = "Authentication required." }),

            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                (object)new { message = "An unexpected error occurred." })
        };

        var correlationId = context.Items["X-Correlation-Id"]?.ToString();

        var body = JsonSerializer.Serialize(new
        {
            title,
            status = (int)statusCode,
            correlationId,
            errors
        }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(body);
    }
}
