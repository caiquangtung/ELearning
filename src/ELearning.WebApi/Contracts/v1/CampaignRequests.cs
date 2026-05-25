namespace ELearning.WebApi.Contracts.v1;

public sealed record ListCampaignsRequest(
    Guid? OrganizationId = null,
    bool IncludeGlobal = true,
    int Page = 1,
    int PageSize = 20);
