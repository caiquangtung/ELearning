using FluentValidation;

namespace ELearning.Application.Features.Notifications.SendAnnouncement;

public sealed class SendAnnouncementCommandValidator : AbstractValidator<SendAnnouncementCommand>
{
    public SendAnnouncementCommandValidator()
    {
        RuleFor(x => x.RecipientUserIds).NotEmpty();
        RuleForEach(x => x.RecipientUserIds).NotEmpty();
        RuleFor(x => x.Subject).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ActionUrl).MaximumLength(1000);
    }
}
