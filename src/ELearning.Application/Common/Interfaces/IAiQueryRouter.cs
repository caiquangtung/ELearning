namespace ELearning.Application.Common.Interfaces;

public enum AiQueryIntentCategory
{
    DirectGreeting,
    QuickCourseLookup,
    TechnicalCodeSample,
    OutOfScope
}

public sealed record AiQueryRouterResult(
    AiQueryIntentCategory Category,
    bool SkipRetrieval,
    string IntentName,
    string? Reason);

public interface IAiQueryRouter
{
    AiQueryRouterResult RouteQuery(string question);
}
