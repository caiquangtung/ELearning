using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.UserAggregate;
using MediatR;

namespace ELearning.Application.Features.Identity.RefreshToken;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork,
    IAuditLogService auditLogs)
    : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = RefreshTokenHasher.Hash(request.RefreshToken);
        var user = await userRepository.GetByRefreshTokenHashAsync(hash, ct);
        if (user is null || !user.IsRefreshTokenValid(hash))
        {
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Auth.RefreshToken",
                "User",
                null,
                "Failure",
                new Dictionary<string, string> { ["reason"] = "invalid_or_expired" }), ct);
            return Result.Failure<AuthResponseDto>(Error.Unauthorized("Invalid or expired refresh token."));
        }

        if (user.Status == UserStatus.Suspended)
        {
            await auditLogs.WriteAsync(new AuditLogEntry(
                "Auth.RefreshToken",
                "User",
                user.Id.ToString(),
                "Failure",
                new Dictionary<string, string> { ["reason"] = "account_suspended" },
                user.Id), ct);
            return Result.Failure<AuthResponseDto>(new Error("User.Suspended", "Account is suspended."));
        }

        var tokens = jwtTokenService.CreateTokenPair(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Roles.ToList());

        var newHash = RefreshTokenHasher.Hash(tokens.RawRefreshToken);
        user.SetRefreshToken(newHash, tokens.RefreshTokenExpiresAtUtc);

        await unitOfWork.SaveChangesAsync(ct);
        await auditLogs.WriteAsync(new AuditLogEntry(
            "Auth.RefreshToken",
            "User",
            user.Id.ToString(),
            "Success",
            ActorUserId: user.Id), ct);

        return new AuthResponseDto(
            tokens.AccessToken,
            tokens.RawRefreshToken,
            tokens.AccessTokenExpiresAtUtc,
            new UserDto(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.FullName,
                user.Roles.ToList()));
    }
}
