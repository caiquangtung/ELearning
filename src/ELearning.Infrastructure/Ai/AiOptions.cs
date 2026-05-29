namespace ELearning.Infrastructure.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string Provider { get; init; } = "Local";
    public string Model { get; init; } = "local-deterministic-v1";
    public string QuizQuestionPromptVersion { get; init; } = "quiz-question-generator-v1";
    public int MaxSourceCharacters { get; init; } = 12000;
}
