using System.Text.Json;
using ELearning.Application.Common.Interfaces;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public sealed class AiRagEvaluationService(
    ApplicationDbContext context,
    IAiKnowledgeRetriever retriever)
    : IAiRagEvaluationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AiRagEvaluationRunSummary> RunAsync(
        Guid? requestedByUserId,
        IReadOnlyCollection<string> userRoles,
        CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        var dataset = LoadDataset();

        try
        {
            var roles = userRoles.Contains(Roles.Admin) ? userRoles : userRoles.Append(Roles.Admin).ToArray();
            var caseResults = new List<RagCaseEvaluationResult>(dataset.Cases.Count);
            foreach (var testCase in dataset.Cases)
                caseResults.Add(await EvaluateCaseAsync(testCase, requestedByUserId ?? Guid.Empty, roles, ct));

            var totalCases = caseResults.Count;
            var passedCases = caseResults.Count(x => x.Passed);
            var inScope = caseResults.Where(x => x.ShouldUseContext).ToList();
            var outOfScope = caseResults.Where(x => !x.ShouldUseContext).ToList();

            var run = AiRagEvaluationRun.Succeeded(
                requestedByUserId,
                dataset.Version,
                totalCases,
                passedCases,
                Rate(inScope.Count(x => x.RetrievalHit), inScope.Count),
                Rate(caseResults.Count(x => x.CitationsValid), totalCases),
                Rate(outOfScope.Count(x => x.RefusalCorrect), outOfScope.Count),
                Rate(inScope.Count(x => x.Grounded), inScope.Count),
                startedAt);

            await context.AiRagEvaluationRuns.AddAsync(run, ct);
            await context.SaveChangesAsync(ct);
            return ToSummary(run);
        }
        catch (Exception ex)
        {
            var run = AiRagEvaluationRun.Failed(
                requestedByUserId,
                dataset.Version,
                ex.Message,
                startedAt);
            await context.AiRagEvaluationRuns.AddAsync(run, ct);
            await context.SaveChangesAsync(ct);
            return ToSummary(run);
        }
    }

    public async Task<IReadOnlyList<AiRagEvaluationRunSummary>> ListAsync(CancellationToken ct = default)
    {
        var runs = await context.AiRagEvaluationRuns
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        return runs.Select(ToSummary).ToList();
    }

    private async Task<RagCaseEvaluationResult> EvaluateCaseAsync(
        RagGoldenCase testCase,
        Guid userId,
        IReadOnlyCollection<string> roles,
        CancellationToken ct)
    {
        var retrieval = await retriever.RetrieveAsync(
            new AiKnowledgeRetrievalRequest(userId, roles, testCase.Question, null),
            ct);
        var citations = retrieval.Citations;
        var citationsValid = await CitationsExistAsync(citations, ct);

        if (!testCase.ShouldUseContext)
        {
            var refused = citations.Count == 0;
            return new RagCaseEvaluationResult(
                false,
                true,
                citationsValid,
                refused,
                true,
                refused && citationsValid);
        }

        var retrievalHit = citations.Any(citation =>
            ContainsAllTerms(citation.CourseTitle, testCase.ExpectedCourseTitleTerms) ||
            ContainsAllTerms(citation.Snippet, testCase.ExpectedSnippetTerms));
        var grounded = citations.Any(citation => ContainsAllTerms(citation.Snippet, testCase.ExpectedSnippetTerms));

        return new RagCaseEvaluationResult(
            true,
            retrievalHit,
            citationsValid,
            true,
            grounded,
            retrievalHit && citationsValid && grounded);
    }

    private async Task<bool> CitationsExistAsync(
        IReadOnlyList<AiChatCitation> citations,
        CancellationToken ct)
    {
        if (citations.Count == 0)
            return true;

        var ids = citations.Select(x => x.ChunkId).ToArray();
        var count = await context.AiKnowledgeChunks.CountAsync(x => ids.Contains(x.Id), ct);
        return count == ids.Length;
    }

    private static bool ContainsAllTerms(string? value, IReadOnlyList<string> terms)
    {
        if (terms.Count == 0)
            return true;
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return terms.All(term => value.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static decimal Rate(int passed, int total) =>
        total == 0 ? 1m : Math.Round((decimal)passed / total, 4);

    private static RagGoldenDataset LoadDataset()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Ai", "Rag", "rag-golden-dataset.json");
        if (File.Exists(path))
        {
            var dataset = JsonSerializer.Deserialize<RagGoldenDataset>(
                File.ReadAllText(path),
                JsonOptions);
            if (dataset?.Cases.Count > 0)
                return dataset;
        }

        return new RagGoldenDataset(
            "rag-golden-v1",
            [
                new RagGoldenCase(
                    "secure-coding-seed-content",
                    "What does the Secure Coding course lesson content cover?",
                    true,
                    ["secure", "coding"],
                    ["seed", "content"]),
                new RagGoldenCase(
                    "out-of-scope-sports",
                    "Who won the 1998 world cup final?",
                    false,
                    [],
                    [])
            ]);
    }

    private static AiRagEvaluationRunSummary ToSummary(AiRagEvaluationRun run) =>
        new(
            run.Id,
            run.Status.ToString(),
            run.RequestedByUserId,
            run.DatasetVersion,
            run.TotalCases,
            run.PassedCases,
            run.RetrievalHitRate,
            run.CitationValidityRate,
            run.RefusalAccuracyRate,
            run.GroundednessRate,
            run.Error,
            run.StartedAt,
            run.CompletedAt,
            run.CreatedAt);

    private sealed record RagGoldenDataset(string Version, List<RagGoldenCase> Cases);

    private sealed record RagGoldenCase(
        string Id,
        string Question,
        bool ShouldUseContext,
        IReadOnlyList<string> ExpectedCourseTitleTerms,
        IReadOnlyList<string> ExpectedSnippetTerms);

    private sealed record RagCaseEvaluationResult(
        bool ShouldUseContext,
        bool RetrievalHit,
        bool CitationsValid,
        bool RefusalCorrect,
        bool Grounded,
        bool Passed);
}
