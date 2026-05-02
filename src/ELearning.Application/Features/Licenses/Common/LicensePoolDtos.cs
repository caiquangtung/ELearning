namespace ELearning.Application.Features.Licenses.Common;

public sealed record LicensePoolListItemDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    int TotalSeats,
    int ActiveSeats,
    int AvailableSeats,
    long SeatPriceCents,
    string Currency,
    DateTime? ExpiresAt,
    DateTime CreatedAt);

public sealed record LicenseAssignmentDto(
    Guid UserId,
    DateTime AssignedAt,
    DateTime? RevokedAt);

public sealed record LicensePoolDetailDto(
    Guid Id,
    Guid OrganizationId,
    string Name,
    int TotalSeats,
    int ActiveSeats,
    int AvailableSeats,
    long SeatPriceCents,
    string Currency,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    IReadOnlyList<LicenseAssignmentDto> Assignments);

public sealed record LicenseUsageReportDto(
    Guid LicensePoolId,
    int TotalSeats,
    int ActiveSeats,
    int AvailableSeats);

