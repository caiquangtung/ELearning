using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.GetTrainingClass;

public sealed class GetTrainingClassQueryHandler(ITrainingClassRepository trainingClassRepository)
    : IRequestHandler<GetTrainingClassQuery, Result<TrainingClassDetailDto>>
{
    public async Task<Result<TrainingClassDetailDto>> Handle(GetTrainingClassQuery request, CancellationToken ct)
    {
        var tc = await trainingClassRepository.GetByIdWithDetailsAsync(request.Id, ct);
        if (tc is null)
            return Result.Failure<TrainingClassDetailDto>(Error.NotFound("TrainingClass", request.Id));

        var instructors = tc.Instructors
            .OrderBy(i => i.AssignedAt)
            .Select(i => new ClassInstructorDto(i.UserId, i.AssignedAt))
            .ToList();

        var sessions = tc.Sessions
            .OrderBy(s => s.StartUtc)
            .Select(s => new ClassSessionDto(
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
                s.UpdatedAt))
            .ToList();

        return new TrainingClassDetailDto(
            tc.Id,
            tc.CourseId,
            tc.Title,
            tc.MaxLearners,
            tc.Status.ToString(),
            tc.CreatedAt,
            tc.UpdatedAt,
            instructors,
            sessions);
    }
}
