using ELearning.Application.Features.Notifications.ListMyNotifications;
using ELearning.Application.Features.Notifications.SendAnnouncement;
using ELearning.Application.Features.Notifications.SendEmail;
using ELearning.Application.Features.Notifications.SendNotification;
using ELearning.Domain.Aggregates.NotificationAggregate;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class NotificationsFeatureSmokeTests
{
    [Fact]
    public void SendNotificationValidator_rejects_empty_recipient()
    {
        var validator = new SendNotificationCommandValidator();

        var result = validator.Validate(new SendNotificationCommand(
            Guid.Empty,
            "Welcome",
            "You have a new notification.",
            NotificationType.Info));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void SendAnnouncementValidator_rejects_empty_recipient_list()
    {
        var validator = new SendAnnouncementCommandValidator();

        var result = validator.Validate(new SendAnnouncementCommand(
            Array.Empty<Guid>(),
            "Course update",
            "New lesson is available.",
            MessageScope.Course));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void SendEmailValidator_requires_valid_email_address()
    {
        var validator = new SendEmailCommandValidator();

        var result = validator.Validate(new SendEmailCommand(
            "not-an-email",
            "Welcome",
            "Thanks for joining."));

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 101)]
    public void ListMyNotificationsValidator_bounds_paging(int page, int pageSize)
    {
        var validator = new ListMyNotificationsQueryValidator();

        var result = validator.Validate(new ListMyNotificationsQuery(page, pageSize, false));

        result.IsValid.Should().BeFalse();
    }
}
