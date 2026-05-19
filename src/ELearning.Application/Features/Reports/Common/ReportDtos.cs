namespace ELearning.Application.Features.Reports.Common;

public sealed record AdminDashboardDto(
    int TotalUsers,
    int ActiveUsers,
    int TotalCourses,
    int PublishedCourses,
    int TotalClasses,
    int ScheduledClasses,
    int PaidOrders,
    int PendingOrders,
    long RevenueCents,
    string Currency,
    int CertificatesIssued);

public sealed record StudentDashboardDto(
    Guid UserId,
    int PaidOrders,
    int CoursePurchases,
    int ClassPurchases,
    int CertificatesIssued,
    int UpcomingSessions);

public sealed record InstructorDashboardDto(
    Guid UserId,
    int AssignedClasses,
    int UpcomingSessions,
    int CompletedSessions,
    int DraftClasses,
    int ScheduledClasses);

public sealed record CourseAnalyticsDto(
    Guid CourseId,
    string Title,
    string Status,
    int ClassCount,
    int CertificateCount,
    int PaidOrderCount,
    long RevenueCents,
    string Currency);

public sealed record OrganizationAnalyticsDto(
    Guid OrganizationId,
    string Name,
    int MemberCount,
    int LicensePoolCount,
    int TotalSeats,
    int ActiveSeats,
    int PaidOrders,
    long RevenueCents,
    string Currency);
