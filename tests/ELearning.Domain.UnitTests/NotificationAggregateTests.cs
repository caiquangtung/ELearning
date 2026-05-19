using ELearning.Domain.Aggregates.NotificationAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class NotificationAggregateTests
{
    [Fact]
    public void Create_notification_sets_unread_recipient_payload()
    {
        var userId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        var notification = Notification.Create(
            userId,
            "  Class reminder  ",
            "  Your session starts tomorrow.  ",
            NotificationType.Reminder,
            "  /classes/123  ",
            messageId);

        notification.UserId.Should().Be(userId);
        notification.MessageId.Should().Be(messageId);
        notification.Title.Should().Be("Class reminder");
        notification.Body.Should().Be("Your session starts tomorrow.");
        notification.Type.Should().Be(NotificationType.Reminder);
        notification.ActionUrl.Should().Be("/classes/123");
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public void Mark_as_read_is_idempotent()
    {
        var notification = Notification.Create(
            Guid.NewGuid(),
            "Welcome",
            "You have been added to a course.");
        var firstReadAt = DateTime.UtcNow;

        notification.MarkAsRead(firstReadAt);
        notification.MarkAsRead(firstReadAt.AddHours(1));

        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().Be(firstReadAt);
    }

    [Fact]
    public void Create_notification_rejects_missing_required_fields()
    {
        var act = () => Notification.Create(Guid.Empty, "", "Body");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_announcement_tracks_scope_and_recipient_count()
    {
        var senderId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var message = Message.CreateAnnouncement(
            senderId,
            "Course update",
            "New lesson is available.",
            MessageScope.Course,
            recipientCount: 12,
            courseId: courseId);

        message.SenderUserId.Should().Be(senderId);
        message.Subject.Should().Be("Course update");
        message.Body.Should().Be("New lesson is available.");
        message.Scope.Should().Be(MessageScope.Course);
        message.CourseId.Should().Be(courseId);
        message.RecipientCount.Should().Be(12);
        message.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_announcement_rejects_empty_recipient_count()
    {
        var act = () => Message.CreateAnnouncement(
            Guid.NewGuid(),
            "Course update",
            "New lesson is available.",
            MessageScope.Course,
            recipientCount: 0);

        act.Should().Throw<DomainException>();
    }
}
