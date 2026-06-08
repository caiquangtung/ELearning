using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Ai;

public static class AiKnowledgeAccessPolicy
{
    public static bool HasPrivilegedKnowledgeAccess(IReadOnlyCollection<string> roles) =>
        roles.Any(role =>
            role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase) ||
            role.Equals(Roles.Instructor, StringComparison.OrdinalIgnoreCase) ||
            role.Equals(Roles.OrgAdmin, StringComparison.OrdinalIgnoreCase));

    public static async Task<List<Guid>> GetAccessiblePublishedCourseIdsAsync(
        ApplicationDbContext context,
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid? courseId,
        CancellationToken ct)
    {
        var publishedCourses = context.Courses
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published)
            .Where(c => !courseId.HasValue || c.Id == courseId.Value);

        if (HasPrivilegedKnowledgeAccess(roles))
            return await publishedCourses.Select(c => c.Id).ToListAsync(ct);

        var freeCourseIds = publishedCourses
            .Where(c => c.PriceCents == 0)
            .Select(c => c.Id);

        var paidCourseIds =
            from order in context.Orders.AsNoTracking()
            join item in context.Set<OrderItem>().AsNoTracking() on order.Id equals item.OrderId
            where order.BuyerUserId == userId
                && order.Status == OrderStatus.Paid
                && item.ItemType == OrderItemType.Course
            select item.ReferenceId;

        var paidClassCourseIds =
            from order in context.Orders.AsNoTracking()
            join item in context.Set<OrderItem>().AsNoTracking() on order.Id equals item.OrderId
            join trainingClass in context.TrainingClasses.AsNoTracking() on item.ReferenceId equals trainingClass.Id
            where order.BuyerUserId == userId
                && order.Status == OrderStatus.Paid
                && item.ItemType == OrderItemType.TrainingClass
            select trainingClass.CourseId;

        var entitledIds = await freeCourseIds
            .Union(paidCourseIds)
            .Union(paidClassCourseIds)
            .ToListAsync(ct);

        var allowedIds = entitledIds.Distinct().ToList();
        return await publishedCourses
            .Where(c => allowedIds.Contains(c.Id))
            .Select(c => c.Id)
            .ToListAsync(ct);
    }
}
