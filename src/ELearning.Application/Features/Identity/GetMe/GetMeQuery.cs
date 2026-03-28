using ELearning.Application.Features.Identity.Login;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.GetMe;

public record GetMeQuery : IRequest<Result<UserDto>>;
