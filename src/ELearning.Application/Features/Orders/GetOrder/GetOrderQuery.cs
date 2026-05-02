using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IRequest<Result<OrderDto>>;

