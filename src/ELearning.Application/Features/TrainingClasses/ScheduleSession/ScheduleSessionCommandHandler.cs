using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.ScheduleSession;

public sealed class ScheduleSessionCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IZoomMeetingService zoomMeetingService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ScheduleSessionCommand, Result<ClassSessionDto>>
{
    public async Task<Result<ClassSessionDto>> Handle(ScheduleSessionCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdWithDetailsAsync(request.TrainingClassId, ct);
        if (trainingClass is null)
            return Result.Failure<ClassSessionDto>(Error.NotFound("TrainingClass", request.TrainingClassId));

        foreach (var instructor in trainingClass.Instructors)
        {
            var overlap = await trainingClassRepository.HasInstructorSessionOverlapAsync(
                instructor.UserId,
                request.StartUtc,
                request.EndUtc,
                excludeSessionId: null,
                ct);
            if (overlap)
                return Result.Failure<ClassSessionDto>(
                    Error.Conflict(
                        "ClassSession",
                        "One or more instructors already have a session overlapping this time range."));
        }

        string? zoomId = null;
        string? zoomUrl = null;
        if (request.SessionType == ClassSessionType.Zoom)
        {
            var meeting = await zoomMeetingService.CreateMeetingAsync(
                request.Title,
                request.StartUtc,
                request.EndUtc,
                ct);
            zoomId = meeting.MeetingId;
            zoomUrl = meeting.JoinUrl;
        }

        ClassSession session;
        try
        {
            session = trainingClass.ScheduleSession(
                request.Title,
                request.SessionType,
                request.StartUtc,
                request.EndUtc,
                request.Location,
                zoomId,
                zoomUrl);
        }
        catch (DomainException ex)
        {
            return Result.Failure<ClassSessionDto>(Error.Validation("Session", ex.Message));
        }

        trainingClassRepository.Update(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return Map(session);
    }

    private static ClassSessionDto Map(ClassSession s) =>
        new(
            s.Id,
            s.Title,
            s.SessionType.ToString(),
            s.StartUtc,
            s.EndUtc,
            s.Location,
            s.ZoomMeetingId,
            s.ZoomJoinUrl,
            s.Status.ToString(),
            s.CreatedAt,
            s.UpdatedAt);
}
