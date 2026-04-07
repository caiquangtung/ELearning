namespace ELearning.Application.Features.TrainingClasses.Common;

public sealed record TrainingClassListItemDto(
    Guid Id,
    Guid CourseId,
    string Title,
    string Status,
    int MaxLearners,
    DateTime CreatedAt);

public sealed record ClassInstructorDto(Guid UserId, DateTime AssignedAt);

public sealed record ClassSessionDto(
    Guid Id,
    string Title,
    string SessionType,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Location,
    string? ZoomMeetingId,
    string? ZoomJoinUrl,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record TrainingClassDetailDto(
    Guid Id,
    Guid CourseId,
    string Title,
    int MaxLearners,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<ClassInstructorDto> Instructors,
    IReadOnlyList<ClassSessionDto> Sessions);
