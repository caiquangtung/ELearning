using ELearning.Application.Features.Licenses.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Licenses.CreateLicensePool;

public sealed record CreateLicensePoolCommand(
    Guid OrganizationId,
    string Name,
    int TotalSeats,
    DateTime? ExpiresAt)
    : IRequest<Result<LicensePoolDetailDto>>;

