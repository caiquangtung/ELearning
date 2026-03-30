using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.GetCourse;

public sealed record GetCourseQuery(Guid Id) : IRequest<Result<CourseDetailDto>>;

