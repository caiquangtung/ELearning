using ELearning.Core.Common;
using ELearning.Application.Features.Identity.Login;
using MediatR;

namespace ELearning.Application.Features.Identity.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<AuthResponseDto>>;
