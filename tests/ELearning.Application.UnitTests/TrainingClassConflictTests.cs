using ELearning.Application.Features.TrainingClasses.ScheduleSession;
using ELearning.Application.Features.TrainingClasses.UpdateSession;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using FluentAssertions;
using NSubstitute;

namespace ELearning.Application.UnitTests;

public class TrainingClassConflictTests
{
    [Fact]
    public async Task ScheduleSession_returns_conflict_when_any_instructor_overlaps()
    {
        var tcId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();

        var tc = TrainingClass.Create(Guid.NewGuid(), "Cohort A", 20);
        tc.AddInstructor(instructorId);

        var repo = Substitute.For<ITrainingClassRepository>();
        repo.GetByIdWithDetailsAsync(tcId, Arg.Any<CancellationToken>()).Returns(tc);
        repo.HasInstructorSessionOverlapAsync(
                instructorId,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                excludeSessionId: null,
                Arg.Any<CancellationToken>())
            .Returns(true);

        var zoom = Substitute.For<IZoomMeetingService>();
        var uow = Substitute.For<IUnitOfWork>();

        var handler = new ScheduleSessionCommandHandler(repo, zoom, uow);
        var cmd = new ScheduleSessionCommand(
            tcId,
            "S1",
            ClassSessionType.Zoom,
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            Location: null);

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("Conflict");
        await zoom.DidNotReceiveWithAnyArgs().CreateMeetingAsync(default!, default);
        await uow.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
    }

    [Fact]
    public async Task UpdateSession_passes_excludeSessionId_to_overlap_check()
    {
        var tcId = Guid.NewGuid();
        var instructorId = Guid.NewGuid();

        var tc = TrainingClass.Create(Guid.NewGuid(), "Cohort A", 20);
        tc.AddInstructor(instructorId);
        var existing = tc.ScheduleSession(
            title: "Old",
            sessionType: ClassSessionType.Offline,
            startUtc: DateTime.UtcNow.AddDays(1),
            endUtc: DateTime.UtcNow.AddDays(1).AddHours(1),
            location: "Room",
            zoomMeetingId: null,
            zoomJoinUrl: null);
        var sessionId = existing.Id;

        var repo = Substitute.For<ITrainingClassRepository>();
        repo.GetByIdWithDetailsAsync(tcId, Arg.Any<CancellationToken>()).Returns(tc);
        repo.HasInstructorSessionOverlapAsync(
                instructorId,
                Arg.Any<DateTime>(),
                Arg.Any<DateTime>(),
                excludeSessionId: sessionId,
                Arg.Any<CancellationToken>())
            .Returns(false);

        var zoom = Substitute.For<IZoomMeetingService>();
        var uow = Substitute.For<IUnitOfWork>();

        var handler = new UpdateSessionCommandHandler(repo, zoom, uow);
        var cmd = new UpdateSessionCommand(
            tcId,
            sessionId,
            "New",
            ClassSessionType.Offline,
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(2).AddHours(1),
            Location: "Room 2");

        var result = await handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await repo.Received(1).HasInstructorSessionOverlapAsync(
            instructorId,
            cmd.StartUtc,
            cmd.EndUtc,
            sessionId,
            Arg.Any<CancellationToken>());
    }
}

