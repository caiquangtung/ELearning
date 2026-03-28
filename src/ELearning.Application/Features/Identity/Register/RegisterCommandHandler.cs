using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Core.Constants;
using ELearning.Domain.Aggregates.UserAggregate;
using MediatR;

namespace ELearning.Application.Features.Identity.Register;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RegisterCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        var email = request.Email.ToLowerInvariant();
        if (await userRepository.ExistsAsync(u => u.Email == email, ct))
            return Result.Failure<AuthResponseDto>(Error.Conflict("User", "Email is already registered."));

        var hash = passwordHasher.Hash(request.Password);
        var user = User.Create(email, hash, request.FirstName, request.LastName, Roles.Learner);

        var tokens = jwtTokenService.CreateTokenPair(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.ToList());

        var refreshHash = RefreshTokenHasher.Hash(tokens.RawRefreshToken);
        user.SetRefreshToken(refreshHash, tokens.RefreshTokenExpiresAtUtc);

        userRepository.Add(user);
        await unitOfWork.SaveChangesAsync(ct);

        var dto = ToUserDto(user);
        return new AuthResponseDto(
            tokens.AccessToken,
            tokens.RawRefreshToken,
            tokens.AccessTokenExpiresAtUtc,
            dto);
    }

    private static UserDto ToUserDto(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName, user.FullName, user.Roles.ToList());
}
