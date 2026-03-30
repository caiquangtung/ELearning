using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Courses.AddLesson;

public sealed class AddLessonToSectionCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddLessonToSectionCommand, Result<LessonDto>>
{
    public async Task<Result<LessonDto>> Handle(AddLessonToSectionCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.CourseId, ct);
        if (course is null)
            return Result.Failure<LessonDto>(Error.NotFound("Course", request.CourseId));

        var section = course.Sections.FirstOrDefault(s => s.Id == request.SectionId);
        if (section is null)
            return Result.Failure<LessonDto>(Error.NotFound("Section", request.SectionId));

        try
        {
            var lesson = section.AddLesson(request.Title);
            courseRepository.Update(course);
            await unitOfWork.SaveChangesAsync(ct);
            return new LessonDto(lesson.Id, lesson.Title, lesson.SortOrder, lesson.Content);
        }
        catch (DomainException ex)
        {
            return Result.Failure<LessonDto>(Error.Validation("Lesson", ex.Message));
        }
    }
}

