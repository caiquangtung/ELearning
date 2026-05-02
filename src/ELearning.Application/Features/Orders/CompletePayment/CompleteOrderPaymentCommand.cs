using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.CompletePayment;

public sealed record CompleteOrderPaymentCommand(string ExternalTransactionId) : IRequest<Result>;
