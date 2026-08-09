using System.Text.RegularExpressions;
using ELearning.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace ELearning.Infrastructure.Ai;

public sealed partial class AiQueryRouter(IOptions<AiOptions> options) : IAiQueryRouter
{
    private static readonly HashSet<string> GreetingTokens = new(StringComparer.OrdinalIgnoreCase)
    {
        "hello", "hi", "hey", "good morning", "good afternoon", "good evening",
        "how are you", "what's up", "greetings", "hi there", "hello there", "hey there",
        "yo", "sup", "howdy", "xin chào", "chào bạn", "chào em", "chào thầy", "chào cô",
        "cảm ơn", "cám ơn", "thanks", "thank you", "bạn là ai", "giới thiệu bản thân"
    };

    private static readonly HashSet<string> OutOfScopeKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "thời tiết", "chứng khoán", "bóng đá", "thể thao", "giá vàng", "tin tức hôm nay",
        "nấu ăn", "xem phim", "showbiz", "dự báo thời tiết", "tỷ giá", "bitcoin", "crypto"
    };

    private static readonly HashSet<string> CodeSampleKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "code", "mẫu code", "ví dụ code", "hàm", "class", "interface", "snippet",
        "ví dụ", "cấu hình", "sample", "example", "implementation", "viết code"
    };

    public AiQueryRouterResult RouteQuery(string question)
    {
        if (!options.Value.RagEnableIntentGating)
        {
            return new AiQueryRouterResult(
                AiQueryIntentCategory.QuickCourseLookup,
                false,
                "QuickCourseLookup",
                "Intent gating disabled by configuration");
        }

        var normalized = question.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return new AiQueryRouterResult(
                AiQueryIntentCategory.OutOfScope,
                true,
                "EmptyQuery",
                "Query is empty or whitespace");
        }

        var lower = normalized.ToLowerInvariant();

        if (IsGreeting(lower))
        {
            return new AiQueryRouterResult(
                AiQueryIntentCategory.DirectGreeting,
                true,
                "DirectGreeting",
                "Recognized greeting or general conversational query");
        }

        if (IsOutOfScope(lower))
        {
            return new AiQueryRouterResult(
                AiQueryIntentCategory.OutOfScope,
                true,
                "OutOfScope",
                "Query matches off-topic or out-of-scope domain");
        }

        if (IsCodeSampleRequest(lower))
        {
            return new AiQueryRouterResult(
                AiQueryIntentCategory.TechnicalCodeSample,
                false,
                "TechnicalCodeSample",
                "Query requests code snippet or hands-on implementation example");
        }

        return new AiQueryRouterResult(
            AiQueryIntentCategory.QuickCourseLookup,
            false,
            "QuickCourseLookup",
            "Standard course domain inquiry");
    }

    private static bool IsGreeting(string lower)
    {
        if (GreetingTokens.Contains(lower))
            return true;

        foreach (var token in GreetingTokens)
        {
            if (lower.StartsWith(token, StringComparison.OrdinalIgnoreCase))
            {
                var nextIndex = token.Length;
                if (nextIndex >= lower.Length || char.IsWhiteSpace(lower[nextIndex]) || lower[nextIndex] is ',' or '!' or '?')
                    return true;
            }
        }

        var words = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= 3 && words.Any(w => GreetingTokens.Contains(w)))
            return true;

        return false;
    }

    private static bool IsOutOfScope(string lower) =>
        OutOfScopeKeywords.Any(kw => lower.Contains(kw, StringComparison.OrdinalIgnoreCase));

    private static bool IsCodeSampleRequest(string lower) =>
        CodeSampleKeywords.Any(kw => lower.Contains(kw, StringComparison.OrdinalIgnoreCase));
}
