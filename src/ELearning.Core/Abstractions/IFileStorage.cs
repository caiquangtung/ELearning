namespace ELearning.Core.Abstractions;

public interface IFileStorage
{
    Task<FileStorageResult> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default);
}

public sealed record FileStorageResult(
    string StorageKey,
    string Url,
    long SizeBytes,
    string FileName,
    string ContentType);

