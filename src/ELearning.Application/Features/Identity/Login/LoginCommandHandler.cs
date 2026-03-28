using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.UserAggregate;
using MediatR;

namespace ELearning.Application.Features.Identity.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var email = request.Email.ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, ct);
        if (user is null)
            return Result.Failure<AuthResponseDto>(Error.Unauthorized("Invalid email or password."));

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponseDto>(Error.Unauthorized("Invalid email or password."));

        if (user.Status == UserStatus.Suspended)
            return Result.Failure<AuthResponseDto>(new Error("User.Suspended", "Account is suspended."));

        var tokens = jwtTokenService.CreateTokenPair(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.ToList());

        var refreshHash = RefreshTokenHasher.Hash(tokens.RawRefreshToken);
        user.SetRefreshToken(refreshHash, tokens.RefreshTokenExpiresAtUtc);

        await unitOfWork.SaveChangesAsync(ct);

        var dto = new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Roles.ToList());

        return new AuthResponseDto(
            tokens.AccessToken,
            tokens.RawRefreshToken,
            tokens.AccessTokenExpiresAtUtc,
            dto);
    }
}
