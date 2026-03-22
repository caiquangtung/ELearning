using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ELearning.Application.Common.Behaviors;

public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var response = await next(ct);
        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
            logger.LogWarning(
                "Slow request detected: {RequestName} took {ElapsedMs}ms",
                typeof(TRequest).Name,
                sw.ElapsedMilliseconds);

        return response;
    }
}
