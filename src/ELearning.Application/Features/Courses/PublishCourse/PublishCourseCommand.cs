using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.PublishCourse;

public sealed record PublishCourseCommand(Guid Id) : IRequest<Result>;

