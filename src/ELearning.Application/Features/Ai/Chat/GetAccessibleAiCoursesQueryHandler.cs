using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Ai.Chat;

public sealed class GetAccessibleAiCoursesQueryHandler(
    IAiRagChatService chatService,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetAccessibleAiCoursesQuery, Result<IReadOnlyList<AiAccessibleCourseDto>>>
{
    public async Task<Result<IReadOnlyList<AiAccessibleCourseDto>>> Handle(
        GetAccessibleAiCoursesQuery request,
        CancellationToken ct)
    {
        if (!currentUserService.UserId.HasValue)
            return Result.Failure<IReadOnlyList<AiAccessibleCourseDto>>(Error.Unauthorized());

        var courses = await chatService.GetAccessibleCoursesAsync(
            currentUserService.UserId.Value,
            currentUserService.Roles.ToArray(),
            ct);

        return courses.Select(c => new AiAccessibleCourseDto(c.Id, c.Title)).ToList();
    }
}
