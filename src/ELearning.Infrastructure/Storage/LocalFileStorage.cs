using ELearning.Core.Abstractions;
using Microsoft.Extensions.Configuration;

namespace ELearning.Infrastructure.Storage;

public sealed class LocalFileStorage(IConfiguration configuration) : IFileStorage
{
    public async Task<FileStorageResult> SaveAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken ct = default)
    {
        var root = configuration["Storage:Local:BasePath"]
            ?? Path.Combine(AppContext.BaseDirectory, "storage");

        Directory.CreateDirectory(root);

        var ext = Path.GetExtension(fileName);
        var storageKey = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(root, storageKey);

        await using (var fs = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, ct);
        }

        var sizeBytes = new FileInfo(fullPath).Length;

        // For now, URL is a logical path; API will serve it later.
        var url = $"/api/v1/assets/{storageKey}";

        return new FileStorageResult(storageKey, url, sizeBytes, fileName, contentType);
    }
}

