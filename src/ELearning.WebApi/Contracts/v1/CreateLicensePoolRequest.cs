namespace ELearning.WebApi.Contracts.v1;

public sealed record CreateLicensePoolRequest(
    string Name,
    int TotalSeats,
    DateTime? ExpiresAt);

