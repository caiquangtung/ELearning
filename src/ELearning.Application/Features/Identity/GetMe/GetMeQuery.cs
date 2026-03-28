using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.GetMe;

public record GetMeQuery : IRequest<Result<UserDto>>;
