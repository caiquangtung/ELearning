using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.UserAggregate;
using MediatR;

namespace ELearning.Application.Features.Identity.Login;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private static readonly Error InvalidCredentials =
        new("Auth.InvalidCredentials", "Invalid email or password.");

    private static readonly Error AccountSuspended =
        new("Auth.AccountSuspended", "Your account has been suspended.");

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponseDto>(InvalidCredentials);

        if (user.Status == UserStatus.Suspended)
            return Result.Failure<AuthResponseDto>(AccountSuspended);

        var tokens = tokenService.GenerateTokenPair(user);
        user.SetRefreshToken(tokenService.HashToken(tokens.RefreshToken), DateTime.UtcNow.AddDays(7));

        await unitOfWork.SaveChangesAsync(ct);

        return BuildResponse(user, tokens);
    }

    private static AuthResponseDto BuildResponse(User user, TokenPair tokens) =>
        new(
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.FullName, user.Roles.ToList()));
}
