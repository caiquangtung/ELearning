using ELearning.Domain.Aggregates.AiAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class AiRequestLogTests
{
    [Fact]
    public void Succeeded_creates_auditable_ai_request_log()
    {
        var log = AiRequestLog.Succeeded(
            Guid.NewGuid(),
            "QuizQuestionGeneration",
            "Local",
            "local-deterministic-v1",
            "quiz-question-generator-v1",
            "abc123",
            42);

        log.Status.Should().Be(AiRequestStatus.Succeeded);
        log.Feature.Should().Be("QuizQuestionGeneration");
        log.InputHash.Should().Be("abc123");
        log.TokenEstimate.Should().Be(42);
        log.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Failed_requires_a_valid_input_hash()
    {
        var act = () => AiRequestLog.Failed(
            null,
            "QuizQuestionGeneration",
            "Local",
            "local-deterministic-v1",
            "quiz-question-generator-v1",
            "",
            "Provider failed.");

        act.Should().Throw<DomainException>();
    }
}
