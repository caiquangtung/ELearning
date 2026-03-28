using ELearning.Application.Features.Identity.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Identity.UpdateProfile;

public record UpdateProfileCommand(string FirstName, string LastName) : IRequest<Result<UserDto>>;
