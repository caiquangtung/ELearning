using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthResponseDto>>;
