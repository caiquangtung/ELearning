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
    private static readonly Error EmailTaken =
        new("Auth.EmailTaken", "An account with this email already exists.");

    public async Task<Result<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await userRepository.EmailExistsAsync(request.Email, ct))
            return Result.Failure<AuthResponseDto>(EmailTaken);

        var passwordHash = passwordHasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash, request.FirstName, request.LastName, Roles.Learner);

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

        return new AuthResponseDto(
            tokens.AccessToken,
            tokens.RawRefreshToken,
            tokens.AccessTokenExpiresAtUtc,
            new UserDto(user.Id, user.Email, user.FirstName, user.LastName, user.FullName, user.Roles.ToList()));
    }
}
