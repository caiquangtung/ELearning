using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Ai.QuizQuestionGeneration;
using ELearning.Infrastructure.Ai;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace ELearning.Application.UnitTests;

public class AiQuizQuestionGenerationTests
{
    [Fact]
    public async Task LocalGenerator_creates_structured_multiple_choice_questions()
    {
        var generator = new LocalQuizQuestionGenerator(Options.Create(new AiOptions()));

        var result = await generator.GenerateAsync(new AiQuizQuestionGenerationRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Secure API Development",
            "Authentication authorization validation and audit logging.",
            "JWT Authentication",
            "JWT authentication validates signed tokens and maps claims to permissions.",
            3,
            "Medium",
            ["MultipleChoice"]));

        result.Provider.Should().Be("Local");
        result.Questions.Should().HaveCount(3);
        result.Questions.Should().OnlyContain(q => q.Type == "MultipleChoice");
        result.Questions.Should().OnlyContain(q => q.Options.Count == 4);
        result.Questions.Should().OnlyContain(q => q.Options.Count(o => o.IsCorrect) == 1);
    }

    [Fact]
    public void GenerateQuizQuestionsValidator_rejects_invalid_question_count()
    {
        var validator = new GenerateQuizQuestionsCommandValidator();

        var result = validator.Validate(new GenerateQuizQuestionsCommand(
            Guid.NewGuid(),
            null,
            11,
            "Medium",
            ["MultipleChoice"]));

        result.Errors.Should().Contain(e => e.PropertyName == "QuestionCount");
    }

    [Fact]
    public void GenerateQuizQuestionsValidator_rejects_unknown_question_type()
    {
        var validator = new GenerateQuizQuestionsCommandValidator();

        var result = validator.Validate(new GenerateQuizQuestionsCommand(
            Guid.NewGuid(),
            null,
            3,
            "Medium",
            ["Matching"]));

        result.Errors.Should().Contain(e => e.PropertyName == "QuestionTypes[0]");
    }
}
