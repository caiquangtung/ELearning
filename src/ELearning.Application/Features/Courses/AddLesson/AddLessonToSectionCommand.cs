using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.AddLesson;

public sealed record AddLessonToSectionCommand(Guid CourseId, Guid SectionId, string Title)
    : IRequest<Result<LessonDto>>;

