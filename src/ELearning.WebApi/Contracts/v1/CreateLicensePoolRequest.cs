namespace ELearning.WebApi.Contracts.v1;

public sealed record ListLicensePoolsRequest(int Page = 1, int PageSize = 20);

public sealed record CreateLicensePoolRequest(
    string Name,
    int TotalSeats,
    DateTime? ExpiresAt);
