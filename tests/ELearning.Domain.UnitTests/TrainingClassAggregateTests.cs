using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class TrainingClassAggregateTests
{
    [Fact]
    public void Create_sets_draft_and_max_learners()
    {
        var courseId = Guid.NewGuid();
        var tc = TrainingClass.Create(courseId, "Cohort A", 25);
        tc.Status.Should().Be(TrainingClassStatus.Draft);
        tc.MaxLearners.Should().Be(25);
        tc.CourseId.Should().Be(courseId);
    }

    [Fact]
    public void ScheduleSession_without_instructor_throws()
    {
        var tc = TrainingClass.Create(Guid.NewGuid(), "Cohort A", 10);
        var act = () => tc.ScheduleSession(
            "Week 1",
            ClassSessionType.Offline,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            "Room 1",
            null,
            null);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ScheduleSession_after_adding_instructor_succeeds()
    {
        var tc = TrainingClass.Create(Guid.NewGuid(), "Cohort A", 10);
        tc.AddInstructor(Guid.NewGuid());

        var session = tc.ScheduleSession(
            "Week 1",
            ClassSessionType.Vod,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            null,
            null,
            null);

        session.Title.Should().Be("Week 1");
        tc.Status.Should().Be(TrainingClassStatus.Scheduled);
        tc.Sessions.Should().ContainSingle();
    }
}
