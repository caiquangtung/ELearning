namespace ELearning.Core.Exceptions;

public class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("One or more validation failures occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
