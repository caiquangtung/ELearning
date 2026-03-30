using ELearning.Application.Features.Courses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Courses.AddSection;

public sealed record AddSectionToCourseCommand(Guid CourseId, string Title)
    : IRequest<Result<SectionDto>>;

