using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.DeleteCourse;

public sealed record DeleteCourseCommand(Guid Id) : IRequest<Result>;

