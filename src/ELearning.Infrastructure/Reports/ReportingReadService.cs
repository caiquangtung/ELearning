using ELearning.Application.Common.Interfaces;
using ELearning.Application.Features.Reports.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Aggregates.UserAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Reports;

public sealed class ReportingReadService(ApplicationDbContext context) : IReportingReadService
{
    public async Task<AdminDashboardDto> GetAdminDashboardAsync(CancellationToken ct = default)
    {
        var paidOrders = context.Orders.AsNoTracking().Where(o => o.Status == OrderStatus.Paid);

        var totalUsers = await context.Users.AsNoTracking().CountAsync(ct);
        var activeUsers = await context.Users.AsNoTracking().CountAsync(u => u.Status == UserStatus.Active, ct);
        var totalCourses = await context.Courses.AsNoTracking().CountAsync(c => !c.IsDeleted, ct);
        var publishedCourses = await context.Courses.AsNoTracking()
            .CountAsync(c => !c.IsDeleted && c.Status == CourseStatus.Published, ct);
        var totalClasses = await context.TrainingClasses.AsNoTracking().CountAsync(c => !c.IsDeleted, ct);
        var scheduledClasses = await context.TrainingClasses.AsNoTracking()
            .CountAsync(c => !c.IsDeleted && c.Status == TrainingClassStatus.Scheduled, ct);
        var paidOrderCount = await paidOrders.CountAsync(ct);
        var pendingOrders = await context.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.PendingPayment, ct);
        var revenueCents = await paidOrders.SumAsync(o => (long?)o.TotalCents, ct) ?? 0;
        var currency = await paidOrders.Select(o => o.Currency).FirstOrDefaultAsync(ct) ?? "USD";
        var certificatesIssued = await context.Certificates.AsNoTracking().CountAsync(ct);

        return new AdminDashboardDto(
            totalUsers,
            activeUsers,
            totalCourses,
            publishedCourses,
            totalClasses,
            scheduledClasses,
            paidOrderCount,
            pendingOrders,
            revenueCents,
            currency,
            certificatesIssued);
    }

    public async Task<StudentDashboardDto> GetStudentDashboardAsync(Guid userId, CancellationToken ct = default)
    {
        var paidOrders = context.Orders.AsNoTracking()
            .Where(o => o.BuyerUserId == userId && o.Status == OrderStatus.Paid);

        var paidOrderIds = await paidOrders.Select(o => o.Id).ToListAsync(ct);
        var paidOrdersCount = paidOrderIds.Count;

        var coursePurchases = await context.Set<OrderItem>().AsNoTracking()
            .Where(i => paidOrderIds.Contains(i.OrderId) && i.ItemType == OrderItemType.Course)
            .Select(i => i.ReferenceId)
            .Distinct()
            .CountAsync(ct);

        var purchasedClassIds = await context.Set<OrderItem>().AsNoTracking()
            .Where(i => paidOrderIds.Contains(i.OrderId) && i.ItemType == OrderItemType.TrainingClass)
            .Select(i => i.ReferenceId)
            .Distinct()
            .ToListAsync(ct);

        var classPurchases = purchasedClassIds.Count;
        var certificatesIssued = await context.Certificates.AsNoTracking().CountAsync(c => c.UserId == userId, ct);
        var now = DateTime.UtcNow;
        var upcomingSessions = purchasedClassIds.Count == 0
            ? 0
            : await context.Set<ClassSession>().AsNoTracking()
                .CountAsync(s => purchasedClassIds.Contains(s.TrainingClassId)
                    && s.Status == ClassSessionStatus.Scheduled
                    && s.StartUtc >= now, ct);

        return new StudentDashboardDto(
            userId,
            paidOrdersCount,
            coursePurchases,
            classPurchases,
            certificatesIssued,
            upcomingSessions);
    }

    public async Task<InstructorDashboardDto> GetInstructorDashboardAsync(Guid userId, CancellationToken ct = default)
    {
        var assignedClasses = await context.TrainingClasses.AsNoTracking()
            .Where(c => !c.IsDeleted && c.Instructors.Any(i => i.UserId == userId))
            .Select(c => new { c.Id, c.Status })
            .ToListAsync(ct);

        var classIds = assignedClasses.Select(c => c.Id).ToList();
        var now = DateTime.UtcNow;
        var upcomingSessions = classIds.Count == 0
            ? 0
            : await context.Set<ClassSession>().AsNoTracking()
                .CountAsync(s => classIds.Contains(s.TrainingClassId)
                    && s.Status == ClassSessionStatus.Scheduled
                    && s.StartUtc >= now, ct);
        var completedSessions = classIds.Count == 0
            ? 0
            : await context.Set<ClassSession>().AsNoTracking()
                .CountAsync(s => classIds.Contains(s.TrainingClassId)
                    && s.Status == ClassSessionStatus.Scheduled
                    && s.EndUtc < now, ct);

        return new InstructorDashboardDto(
            userId,
            assignedClasses.Count,
            upcomingSessions,
            completedSessions,
            assignedClasses.Count(c => c.Status == TrainingClassStatus.Draft),
            assignedClasses.Count(c => c.Status == TrainingClassStatus.Scheduled));
    }

    public async Task<CourseAnalyticsDto?> GetCourseAnalyticsAsync(Guid courseId, CancellationToken ct = default)
    {
        var course = await context.Courses.AsNoTracking()
            .Where(c => c.Id == courseId && !c.IsDeleted)
            .Select(c => new { c.Id, c.Title, c.Status, c.Currency })
            .FirstOrDefaultAsync(ct);
        if (course is null)
            return null;

        var classCount = await context.TrainingClasses.AsNoTracking()
            .CountAsync(c => !c.IsDeleted && c.CourseId == courseId, ct);
        var certificateCount = await context.Certificates.AsNoTracking()
            .CountAsync(c => c.CourseId == courseId, ct);

        var paidCourseItems = from item in context.Set<OrderItem>().AsNoTracking()
                              join order in context.Orders.AsNoTracking() on item.OrderId equals order.Id
                              where order.Status == OrderStatus.Paid
                                  && item.ItemType == OrderItemType.Course
                                  && item.ReferenceId == courseId
                              select new { item.OrderId, item.UnitPriceCents, item.Quantity, order.Currency };

        var paidOrderCount = await paidCourseItems.Select(i => i.OrderId).Distinct().CountAsync(ct);
        var revenueCents = await paidCourseItems.SumAsync(i => (long?)(i.UnitPriceCents * i.Quantity), ct) ?? 0;
        var currency = await paidCourseItems.Select(i => i.Currency).FirstOrDefaultAsync(ct) ?? course.Currency;

        return new CourseAnalyticsDto(
            course.Id,
            course.Title,
            course.Status.ToString(),
            classCount,
            certificateCount,
            paidOrderCount,
            revenueCents,
            currency);
    }

    public async Task<OrganizationAnalyticsDto?> GetOrganizationAnalyticsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var organization = await context.Organizations.AsNoTracking()
            .Where(o => o.Id == organizationId)
            .Select(o => new
            {
                o.Id,
                o.Name,
                MemberCount = o.Members.Count
            })
            .FirstOrDefaultAsync(ct);
        if (organization is null)
            return null;

        var pools = await context.LicensePools.AsNoTracking()
            .Where(p => p.OrganizationId == organizationId)
            .Select(p => new
            {
                p.TotalSeats,
                ActiveSeats = p.Assignments.Count(a => a.RevokedAt == null)
            })
            .ToListAsync(ct);

        var paidOrders = context.Orders.AsNoTracking()
            .Where(o => o.OrganizationId == organizationId && o.Status == OrderStatus.Paid);
        var paidOrderCount = await paidOrders.CountAsync(ct);
        var revenueCents = await paidOrders.SumAsync(o => (long?)o.TotalCents, ct) ?? 0;
        var currency = await paidOrders.Select(o => o.Currency).FirstOrDefaultAsync(ct) ?? "USD";

        return new OrganizationAnalyticsDto(
            organization.Id,
            organization.Name,
            organization.MemberCount,
            pools.Count,
            pools.Sum(p => p.TotalSeats),
            pools.Sum(p => p.ActiveSeats),
            paidOrderCount,
            revenueCents,
            currency);
    }
}
