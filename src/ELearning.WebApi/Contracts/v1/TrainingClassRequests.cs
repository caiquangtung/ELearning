namespace ELearning.WebApi.Contracts.v1;

public sealed record ListTrainingClassesRequest(
    int Page = 1,
    int PageSize = 20,
    Guid? CourseId = null,
    string? Search = null);

public sealed record CreateTrainingClassRequest(Guid CourseId, string Title, int MaxLearners);

public sealed record UpdateTrainingClassRequest(string Title, int MaxLearners);

public sealed record AssignInstructorRequest(Guid UserId);

public sealed record ScheduleSessionRequest(
    string Title,
    string SessionType,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Location);

public sealed record UpdateSessionRequest(
    string Title,
    string SessionType,
    DateTime StartUtc,
    DateTime EndUtc,
    string? Location);
