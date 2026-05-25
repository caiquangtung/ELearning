namespace ELearning.WebApi.Contracts.v1;

public sealed record ListCourseReviewsRequest(int Page = 1, int PageSize = 20, bool IncludeRejected = false);

public sealed record SubmitReviewRequest(int Rating, string Comment);

public sealed record ModerateReviewRequest(string Status, string? Reason);
