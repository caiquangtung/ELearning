using ELearning.Application.Common.Interfaces;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Exceptions;
using MediatR;

namespace ELearning.Application.Features.Orders.CompletePayment;

public sealed class CompleteOrderPaymentCommandHandler(
    IOrderPaymentRepository paymentRepository,
    IOrderRepository orderRepository,
    IInvoiceRepository invoiceRepository,
    ICheckoutReservationRepository reservationRepository,
    ICouponRepository couponRepository,
    ICouponRedemptionRepository couponRedemptionRepository,
    ICouponUsageReservationRepository couponUsageReservationRepository,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteOrderPaymentCommand, Result>
{
    public async Task<Result> Handle(CompleteOrderPaymentCommand request, CancellationToken ct)
    {
        try
        {
            var payment = await paymentRepository.GetByExternalTransactionIdAsync(request.ExternalTransactionId, ct);
            if (payment is null)
                return Result.Failure(Error.NotFound(nameof(OrderPayment), request.ExternalTransactionId));

            if (payment.Status == OrderPaymentStatus.Succeeded)
                return Result.Success();

            if (!await paymentService.VerifyPaymentAsync(request.ExternalTransactionId, ct))
                return Result.Failure(Error.Conflict("Payment.VerificationFailed", "Payment could not be verified."));

            var order = await orderRepository.GetByIdWithItemsAsync(payment.OrderId, ct);
            if (order is null)
                return Result.Failure(Error.NotFound(nameof(Order), payment.OrderId));

            var utcNow = DateTime.UtcNow;

            if (order.TryExpireCheckout(utcNow))
            {
                await reservationRepository.ReleaseForOrderAsync(order.Id, ct);
                await unitOfWork.SaveChangesAsync(ct);
                return Result.Failure(Error.Conflict("Order.CheckoutExpired", "Checkout window expired; order was cancelled."));
            }

            if (order.Status == OrderStatus.Paid)
                return Result.Success();

            if (order.Status != OrderStatus.PendingPayment)
                return Result.Failure(Error.Conflict("Order.InvalidState", $"Order is not awaiting payment (status {order.Status})."));

            if (payment.AmountCents != order.TotalCents ||
                !payment.Currency.Equals(order.Currency, StringComparison.OrdinalIgnoreCase))
                return Result.Failure(Error.Conflict("Payment.AmountMismatch", "Payment amount does not match order total."));

            order.MarkPaid();
            payment.MarkSucceeded();

            var existingInvoice = await invoiceRepository.GetByOrderIdAsync(order.Id, ct);
            if (existingInvoice is null)
            {
                var invoiceNumber = $"INV-{utcNow:yyyyMMdd}-{order.Id:N}";
                invoiceRepository.Add(Invoice.Issue(order.Id, invoiceNumber, order.Currency, order.TotalCents));
            }

            await reservationRepository.ReleaseForOrderAsync(order.Id, ct);
            await couponUsageReservationRepository.ReleaseForOrderAsync(order.Id, ct);

            if (!string.IsNullOrWhiteSpace(order.AppliedCouponCode))
            {
                var normalized = Domain.Aggregates.PromotionAggregate.Coupon.NormalizeCode(order.AppliedCouponCode);
                var coupon = await couponRepository.GetByCodeNormalizedAsync(normalized, ct);
                if (coupon is not null)
                    couponRedemptionRepository.AddRedemption(coupon.Id, order.BuyerUserId, order.Id, utcNow);
            }

            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(Error.Conflict("Payment.CompletionFailed", ex.Message));
        }
    }
}
