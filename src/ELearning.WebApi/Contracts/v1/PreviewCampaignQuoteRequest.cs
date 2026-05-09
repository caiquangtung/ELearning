namespace ELearning.WebApi.Contracts.v1;

public sealed record PreviewCampaignQuoteItemRequest(
    string ItemType,
    Guid ReferenceId,
    int Quantity);

public sealed record PreviewCampaignQuoteRequest(
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Currency,
    IReadOnlyList<PreviewCampaignQuoteItemRequest> Items,
    string? CouponCode);

