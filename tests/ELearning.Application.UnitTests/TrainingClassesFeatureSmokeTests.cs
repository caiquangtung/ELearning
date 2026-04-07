using ELearning.Application.Features.TrainingClasses.ScheduleSession;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class TrainingClassesFeatureSmokeTests
{
    [Fact]
    public void ScheduleSessionValidator_requires_location_for_offline()
    {
        var v = new ScheduleSessionCommandValidator();
        var cmd = new ScheduleSessionCommand(
            Guid.NewGuid(),
            "S1",
            ClassSessionType.Offline,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            Location: null);
        var result = v.Validate(cmd);
        result.IsValid.Should().BeFalse();
    }
}
