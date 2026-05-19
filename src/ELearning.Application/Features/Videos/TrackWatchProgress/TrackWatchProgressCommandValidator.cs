using FluentValidation;

namespace ELearning.Application.Features.Videos.TrackWatchProgress;

public sealed class TrackWatchProgressCommandValidator : AbstractValidator<TrackWatchProgressCommand>
{
    public TrackWatchProgressCommandValidator()
    {
        RuleFor(x => x.VideoAssetId).NotEmpty();
        RuleFor(x => x.PositionSeconds).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DurationSeconds).GreaterThan(0);
        RuleFor(x => x.WatchedSeconds).GreaterThanOrEqualTo(0);
    }
}
