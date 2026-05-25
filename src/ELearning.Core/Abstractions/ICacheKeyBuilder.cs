namespace ELearning.Core.Abstractions;

public interface ICacheKeyBuilder
{
    string Build(params string[] parts);
    string BuildHashKey(string prefix, object value);
}
