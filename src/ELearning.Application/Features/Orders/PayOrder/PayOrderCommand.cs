using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.PayOrder;

public sealed record PayOrderCommand(Guid OrderId) : IRequest<Result<OrderDto>>;
