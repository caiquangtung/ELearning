namespace ELearning.WebApi.Contracts.v1;

public sealed record ListMyOrdersRequest(Guid BuyerUserId, int Page = 1, int PageSize = 20);

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
