using ELearning.Domain.Aggregates.CourseAggregate;

namespace ELearning.WebApi.Contracts.v1;

public sealed record CreateCourseRequest(string Title, string? Description);

public sealed record UpdateCourseRequest(string Title, string? Description);

public sealed record AddSectionRequest(string Title);

public sealed record AddLessonRequest(string Title);

public sealed record UploadAssetRequest(ContentAssetType AssetType, IFormFile File);

