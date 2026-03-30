# File Upload Storage Provider Guide (Local -> S3/Azure Blob)

Sprint 2 uses `IFileStorage` to store lesson uploads (video/PDF/SCORM). Swapping providers usually means:

1. Replacing the `IFileStorage` implementation
2. Ensuring the returned `url` is usable by clients
3. (If needed) adjusting/replacing `GET /api/v1/assets/{storageKey}` (currently local-disk only)

## Summary

- Swapping storage is **easy at upload time** because the upload handler depends only on `IFileStorage.SaveAsync(...)`.
- The main thing to watch when moving to S3/Azure is **how files are served**: `AssetsController` currently streams from local disk. Recommended approach is to return a direct URL (public or presigned) from the provider and let the client fetch it.

## 1. Is it easy to replace?

**Yes for uploads**, because the command handler only depends on `IFileStorage.SaveAsync(...)`:

- `UploadLessonAssetCommandHandler` calls `fileStorage.SaveAsync(...)`
- A new provider only needs to implement `IFileStorage`
- The `content_assets` table still stores `storage_key`, and the API responds with `url` from the provider

**Slightly more involved for “serving files”**, because today:

- `LocalFileStorage` writes to disk under `Storage:Local:BasePath`
- `AssetsController` serves local disk files via `GET /api/v1/assets/{storageKey}`

If you move to S3/Azure and still want `GET /api/v1/assets/...`, you must either:

- Implement a proxy/controller that streams from S3/Azure, or
- (Recommended) have the provider return a direct URL (public or presigned) and have clients use that URL.

## 2. `IFileStorage` contract

Providers return `FileStorageResult`:

- `StorageKey`: persisted to DB
- `Url`: what clients will use to download/view
- `SizeBytes`, `FileName`, `ContentType`: used to build `ContentAssetDto`

## 3. How to switch Local -> S3/Azure (recommended)

### Step A: Create a new provider

Create a new class implementing `IFileStorage`, e.g.:

- `S3FileStorage : IFileStorage`
- `AzureBlobFileStorage : IFileStorage`

The provider should:

- Upload `Stream content` to your bucket/container
- Generate a unique `storageKey` (e.g. `guid.ext`)
- Return `FileStorageResult` with:
  - `StorageKey = storageKey`
  - `Url = <public or presigned URL>`

> Note: if you use short-lived presigned URLs, generating them inside `SaveAsync` means the URL will expire later. In that case you may want to store only `storageKey` and generate presigned URLs on-demand via a separate endpoint (out of current scope).

### Step B: Register the provider in DI

Today DI binds:

- `services.AddSingleton<IFileStorage, LocalFileStorage>();`

Swap it to the new provider (optionally controlled by config/feature flag), e.g.:

- `Storage:Provider = "Local" | "S3" | "AzureBlob"`

### Step C: Ensure clients use the correct URL

After switching:

- Upload responses return `url` from the provider
- Clients should use `contentAsset.url` instead of always calling `/api/v1/assets/{storageKey}`

You can keep `AssetsController` (it won’t be used by S3/Azure URLs) or remove/disable it to avoid confusion.

## 4. Where is the Local provider configured?

`AssetsController` and `LocalFileStorage` read `Storage:Local:BasePath`:

```json
{
  "Storage": {
    "Local": {
      "BasePath": "/absolute/path/to/ELearning-storage"
    }
  }
}
```

If `BasePath` is not set, the system falls back to:

- `AppContext.BaseDirectory / storage`

## 5. Provider swap checklist

1. Implement `IFileStorage.SaveAsync` to upload the stream
2. Return a usable `Url` (public or presigned)
3. Swap the DI binding for `IFileStorage`
4. Ensure the frontend uses `asset.url` (no hardcoded `/api/v1/assets/...`)
5. If you still want `GET /api/v1/assets/{storageKey}` for S3/Azure, implement a proxy/controller

