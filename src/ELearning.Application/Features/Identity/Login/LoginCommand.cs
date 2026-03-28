using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponseDto>>;
