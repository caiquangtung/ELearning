using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.UpdateCourse;

public sealed record UpdateCourseCommand(Guid Id, string Title, string? Description)
    : IRequest<Result<CourseDto>>;

