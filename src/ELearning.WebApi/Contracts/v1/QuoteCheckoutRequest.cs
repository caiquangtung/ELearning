namespace ELearning.WebApi.Contracts.v1;

public sealed record QuoteCheckoutItemRequest(
    string ItemType,
    Guid ReferenceId,
    int Quantity);

public sealed record QuoteCheckoutRequest(
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Currency,
    IReadOnlyList<QuoteCheckoutItemRequest> Items,
    string? CouponCode);

