using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.UserAggregate;
using MediatR;

namespace ELearning.Application.Features.Identity.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = RefreshTokenHasher.Hash(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(hash, ct);
        if (user is null || !user.IsRefreshTokenValid(hash))
            return Result.Failure<AuthResponseDto>(Error.Unauthorized("Invalid or expired refresh token."));

        if (user.Status == UserStatus.Suspended)
            return Result.Failure<AuthResponseDto>(new Error("User.Suspended", "Account is suspended."));

        var tokens = jwtTokenService.CreateTokenPair(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.ToList());

        var newHash = RefreshTokenHasher.Hash(tokens.RawRefreshToken);
        user.SetRefreshToken(newHash, tokens.RefreshTokenExpiresAtUtc);

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
