using ELearning.Domain.Aggregates.CourseAggregate;

namespace ELearning.WebApi.Contracts.v1;

public sealed record ListCoursesRequest(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string? Status = null,
    long? MinPriceCents = null,
    long? MaxPriceCents = null,
    string? Sort = null);

public sealed record CreateCourseRequest(string Title, string? Description);

public sealed record UpdateCourseRequest(string Title, string? Description);

public sealed record AddSectionRequest(string Title);

public sealed record AddLessonRequest(string Title);

public sealed record UploadAssetRequest(ContentAssetType AssetType, IFormFile File);
