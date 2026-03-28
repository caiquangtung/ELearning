using System.Security.Claims;
using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private static readonly Error InvalidToken =
        new("Auth.InvalidToken", "The provided token pair is invalid or expired.");

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var principal = tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null) return Result.Failure<AuthResponseDto>(InvalidToken);

        var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdStr, out var userId))
            return Result.Failure<AuthResponseDto>(InvalidToken);

        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null) return Result.Failure<AuthResponseDto>(InvalidToken);

        var incomingHash = tokenService.HashToken(request.RefreshToken);
        if (!user.IsRefreshTokenValid(incomingHash))
            return Result.Failure<AuthResponseDto>(InvalidToken);

        var tokens = tokenService.GenerateTokenPair(user);
        user.SetRefreshToken(tokenService.HashToken(tokens.RefreshToken), DateTime.UtcNow.AddDays(7));

        await unitOfWork.SaveChangesAsync(ct);

        return new AuthResponseDto(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.FullName, user.Roles.ToList()));
    }
}
