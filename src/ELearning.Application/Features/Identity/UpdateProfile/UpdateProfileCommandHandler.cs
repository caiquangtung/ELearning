using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.UpdateProfile;

public class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    ICurrentUserService currentUser,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateProfileCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(UpdateProfileCommand request, CancellationToken ct)
    {
        if (currentUser.UserId is null)
            return Result.Failure<UserDto>(Error.Unauthorized());

        var user = await userRepository.GetByIdAsync(currentUser.UserId.Value, ct);
        if (user is null)
            return Result.Failure<UserDto>(Error.NotFound("User", currentUser.UserId.Value));

        user.UpdateProfile(request.FirstName, request.LastName);
        await unitOfWork.SaveChangesAsync(ct);

        return new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Roles);
    }
}
