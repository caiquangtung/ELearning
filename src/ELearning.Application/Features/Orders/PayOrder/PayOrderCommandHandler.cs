using ELearning.Application.Common.Interfaces;
using ELearning.Application.Common.Options;
using ELearning.Application.Features.Orders.Common;
using ELearning.Application.Features.Orders.CompletePayment;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using ELearning.Domain.Aggregates.CommerceAggregate;
using ELearning.Domain.Aggregates.OrderAggregate;
using MediatR;
using Microsoft.Extensions.Options;

namespace ELearning.Application.Features.Orders.PayOrder;

public sealed class PayOrderCommandHandler(
    IOrderRepository orderRepository,
    IOrderPaymentRepository paymentRepository,
    ICheckoutReservationRepository reservationRepository,
    IPaymentService paymentService,
    IUnitOfWork unitOfWork,
    ISender mediator,
    IOptions<PaymentOptions> paymentOptions)
    : IRequestHandler<PayOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(PayOrderCommand request, CancellationToken ct)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(request.OrderId, ct);
        if (order is null)
            return Result.Failure<OrderDto>(Error.NotFound(nameof(Order), request.OrderId));

        var utcNow = DateTime.UtcNow;

        if (order.TryExpireCheckout(utcNow))
        {
            await reservationRepository.ReleaseForOrderAsync(order.Id, ct);
            await unitOfWork.SaveChangesAsync(ct);
            return Result.Failure<OrderDto>(Error.Conflict("Order.CheckoutExpired", "Checkout expired; order was cancelled."));
        }

        if (order.Status == OrderStatus.Paid)
            return Result.Success(OrderDtoMapper.ToDto(order));

        if (order.Status != OrderStatus.PendingPayment)
            return Result.Failure<OrderDto>(Error.Conflict("Order.InvalidState", $"Order cannot be paid (status {order.Status})."));

        var pendingPayments = await paymentRepository.FindAsync(
            p => p.OrderId == order.Id && p.Status == OrderPaymentStatus.Pending,
            ct);

        OrderPayment paymentRecord;
        var pending = pendingPayments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();
        if (pending is not null)
        {
            paymentRecord = pending;
        }
        else
        {
            var amountDecimal = order.TotalCents / 100m;
            var paymentResult = await paymentService.CreatePaymentAsync(
                new PaymentRequest(order.Id, amountDecimal, order.Currency, $"Order {order.Id}"),
                ct);

            paymentRecord = OrderPayment.CreatePending(
                order.Id,
                order.TotalCents,
                order.Currency,
                paymentOptions.Value.Provider,
                paymentResult.TransactionId);

            paymentRepository.Add(paymentRecord);
            await unitOfWork.SaveChangesAsync(ct);
        }

        if (!await paymentService.VerifyPaymentAsync(paymentRecord.ExternalTransactionId, ct))
            return Result.Failure<OrderDto>(Error.Conflict("Payment.VerificationFailed", "Payment verification failed."));

        var complete = await mediator.Send(new CompleteOrderPaymentCommand(paymentRecord.ExternalTransactionId), ct);
        if (complete.IsFailure)
            return Result.Failure<OrderDto>(complete.Error);

        var refreshed = await orderRepository.GetByIdWithItemsAsync(order.Id, ct);
        return Result.Success(OrderDtoMapper.ToDto(refreshed!));
    }
}
