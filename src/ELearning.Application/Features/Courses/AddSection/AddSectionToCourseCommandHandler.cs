using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Courses.AddSection;

public sealed class AddSectionToCourseCommandHandler(
    ICourseRepository courseRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddSectionToCourseCommand, Result<SectionDto>>
{
    public async Task<Result<SectionDto>> Handle(AddSectionToCourseCommand request, CancellationToken ct)
    {
        var course = await courseRepository.GetByIdWithDetailsAsync(request.CourseId, ct);
        if (course is null)
            return Result.Failure<SectionDto>(Error.NotFound("Course", request.CourseId));

        try
        {
            var section = course.AddSection(request.Title);
            courseRepository.Update(course);
            await unitOfWork.SaveChangesAsync(ct);
            return new SectionDto(section.Id, section.Title, section.SortOrder);
        }
        catch (DomainException ex)
        {
            return Result.Failure<SectionDto>(Error.Validation("Section", ex.Message));
        }
    }
}

