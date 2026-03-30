using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.CreateCourse;

public sealed record CreateCourseCommand(string Title, string? Description)
    : IRequest<Result<CourseDto>>;

