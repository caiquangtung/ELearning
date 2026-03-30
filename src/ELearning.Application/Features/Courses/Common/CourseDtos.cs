using ELearning.Domain.Aggregates.CourseAggregate;

namespace ELearning.Application.Features.Courses.Common;

public sealed record CourseDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CourseListItemDto(
    Guid Id,
    string Title,
    string Status,
    DateTime CreatedAt);

public sealed record SectionDto(
    Guid Id,
    string Title,
    int SortOrder);

public sealed record LessonDto(
    Guid Id,
    string Title,
    int SortOrder,
    string? Content);

public sealed record ContentAssetDto(
    Guid Id,
    ContentAssetType AssetType,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Url,
    DateTime UploadedAt);

public sealed record CourseDetailDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<CourseSectionDetailDto> Sections);

public sealed record CourseSectionDetailDto(
    Guid Id,
    string Title,
    int SortOrder,
    IReadOnlyList<CourseLessonDetailDto> Lessons);

public sealed record CourseLessonDetailDto(
    Guid Id,
    string Title,
    int SortOrder,
    string? Content,
    IReadOnlyList<ContentAssetDto> Assets);

