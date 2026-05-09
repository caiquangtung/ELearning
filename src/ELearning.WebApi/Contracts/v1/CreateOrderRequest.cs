namespace ELearning.WebApi.Contracts.v1;

public sealed record CreateOrderItemRequest(
    string ItemType,
    Guid ReferenceId,
    int Quantity,
    long UnitPriceCents);

public sealed record CreateOrderRequest(
    Guid BuyerUserId,
    Guid? OrganizationId,
    string Currency,
    IReadOnlyList<CreateOrderItemRequest> Items,
    long DiscountCents = 0,
    string? CouponCode = null);

