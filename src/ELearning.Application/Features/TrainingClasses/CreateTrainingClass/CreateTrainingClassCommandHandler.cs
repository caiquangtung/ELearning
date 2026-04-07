using ELearning.Application.Features.TrainingClasses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using MediatR;

namespace ELearning.Application.Features.TrainingClasses.CreateTrainingClass;

public sealed class CreateTrainingClassCommandHandler(
    ICourseRepository courseRepository,
    ITrainingClassRepository trainingClassRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTrainingClassCommand, Result<TrainingClassListItemDto>>
{
    public async Task<Result<TrainingClassListItemDto>> Handle(CreateTrainingClassCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdAsync(request.CourseId, ct);
        if (course is null)
            return Result.Failure<TrainingClassListItemDto>(Error.NotFound("Course", request.CourseId));

        if (course.Status != CourseStatus.Published)
            return Result.Failure<TrainingClassListItemDto>(
                Error.Validation("CourseId", "Course must be published before creating a scheduled class."));

        var trainingClass = TrainingClass.Create(request.CourseId, request.Title, request.MaxLearners);
        trainingClassRepository.Add(trainingClass);
        await unitOfWork.SaveChangesAsync(ct);

        return new TrainingClassListItemDto(
            trainingClass.Id,
            trainingClass.CourseId,
            trainingClass.Title,
            trainingClass.Status.ToString(),
            trainingClass.MaxLearners,
            trainingClass.CreatedAt);
    }
}
