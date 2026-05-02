using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.GetInvoice;

public sealed record GetInvoiceByOrderQuery(Guid OrderId) : IRequest<Result<InvoiceDto>>;
