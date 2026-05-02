namespace ELearning.Application.Features.Orders;

public static class CommerceConstants
{
    /// <summary>Checkout window for pending orders (seat reservations mirror this).</summary>
    public static readonly TimeSpan CheckoutTimeout = TimeSpan.FromMinutes(15);
}
