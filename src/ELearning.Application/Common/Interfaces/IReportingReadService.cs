using ELearning.Application.Features.Reports.Common;

namespace ELearning.Application.Common.Interfaces;

public interface IReportingReadService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default);
    Task<StudentDashboardDto> GetStudentDashboardAsync(Guid userId, CancellationToken ct = default);
    Task<InstructorDashboardDto> GetInstructorDashboardAsync(Guid userId, CancellationToken ct = default);
    Task<CourseAnalyticsDto?> GetCourseAnalyticsAsync(Guid courseId, CancellationToken ct = default);
    Task<OrganizationAnalyticsDto?> GetOrganizationAnalyticsAsync(Guid organizationId, CancellationToken ct = default);
}
