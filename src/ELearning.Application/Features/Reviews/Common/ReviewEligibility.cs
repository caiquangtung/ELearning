using ELearning.Core.Abstractions;

namespace ELearning.Application.Features.Reviews.Common;

public sealed record ReviewEligibilityDto(Guid CourseId, bool CanReview, string? Reason);

internal static class ReviewEligibility
{
    public const string CompletionRequiredReason = "Complete the course before submitting a review.";

    public static async Task<bool> CanReviewAsync(
        Guid courseId,
        Guid userId,
        ICourseRepository courseRepository,
        ICertificateRepository certificateRepository,
        IVideoAssetRepository videoAssetRepository,
        IWatchEventRepository watchEventRepository,
        CancellationToken ct)
    {
        if (await certificateRepository.ExistsVerifiableForCourseAsync(userId, courseId, ct))
            return true;

        var course = await courseRepository.GetByIdWithDetailsAsync(courseId, ct);
        if (course is null)
            return false;

        var lessonIds = course.Sections
            .SelectMany(s => s.Lessons)
            .Select(l => l.Id)
            .Distinct()
            .ToList();

        var videos = await videoAssetRepository.ListByLessonIdsAsync(lessonIds, ct);
        if (videos.Count == 0)
            return false;

        var videoIds = videos.Select(v => v.Id).Distinct().ToList();
        var completedCount = await watchEventRepository.CountCompletedForVideosAsync(videoIds, userId, ct);
        return completedCount >= videoIds.Count;
    }
}
