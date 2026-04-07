using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.UpdateSession;

public sealed class UpdateSessionCommandHandler(
    ITrainingClassRepository trainingClassRepository,
    IZoomMeetingService zoomMeetingService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateSessionCommand, Result<ClassSessionDto>>
{
    public async Task<Result<ClassSessionDto>> Handle(UpdateSessionCommand request, CancellationToken ct)
    {
        var trainingClass = await trainingClassRepository.GetByIdWithDetailsAsync(request.TrainingClassId, ct);
        if (trainingClass is null)
            return Result.Failure<ClassSessionDto>(Error.NotFound("TrainingClass", request.TrainingClassId));

        var session = trainingClass.Sessions.FirstOrDefault(s => s.Id == request.SessionId);
        if (session is null)
            return Result.Failure<ClassSessionDto>(Error.NotFound("ClassSession", request.SessionId));

        foreach (var instructor in trainingClass.Instructors)
        {
            var overlap = await trainingClassRepository.HasInstructorSessionOverlapAsync(
                instructor.UserId,
                request.StartUtc,
                request.EndUtc,
                excludeSessionId: request.SessionId,
                ct);
            if (overlap)
                return Result.Failure<ClassSessionDto>(
                    Error.Conflict(
                        "ClassSession",
                        "One or more instructors already have a session overlapping this time range."));
        }

        string? zoomId = session.ZoomMeetingId;
        string? zoomUrl = session.ZoomJoinUrl;
        if (request.SessionType == ClassSessionType.Zoom &&
            (string.IsNullOrEmpty(zoomId) ||
             session.StartUtc != request.StartUtc ||
             session.EndUtc != request.EndUtc ||
             session.SessionType != ClassSessionType.Zoom))
        {
            var meeting = await zoomMeetingService.CreateMeetingAsync(
                request.Title,
                request.StartUtc,
                request.EndUtc,
                ct);
            zoomId = meeting.MeetingId;
            zoomUrl = meeting.JoinUrl;
        }

        if (request.SessionType != ClassSessionType.Zoom)
        {
            zoomId = null;
            zoomUrl = null;
        }

        try
        {
            session.UpdateSchedule(
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

        return new ClassSessionDto(
            session.Id,
            session.Title,
            session.SessionType.ToString(),
            session.StartUtc,
            session.EndUtc,
            session.Location,
            session.ZoomMeetingId,
            session.ZoomJoinUrl,
            session.Status.ToString(),
            session.CreatedAt,
            session.UpdatedAt);
    }
}
