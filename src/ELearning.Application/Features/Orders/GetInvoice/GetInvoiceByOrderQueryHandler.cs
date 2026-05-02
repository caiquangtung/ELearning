using ELearning.Application.Features.Orders.Common;
using ELearning.Core.Abstractions;
using ELearning.Core.Common;
using MediatR;

namespace ELearning.Application.Features.Orders.GetInvoice;

public sealed class GetInvoiceByOrderQueryHandler(IInvoiceRepository invoices)
    : IRequestHandler<GetInvoiceByOrderQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByOrderQuery request, CancellationToken ct)
    {
        var inv = await invoices.GetByOrderIdAsync(request.OrderId, ct);
        if (inv is null)
            return Result.Failure<InvoiceDto>(Error.NotFound("Invoice", request.OrderId));

        return new InvoiceDto(
            inv.Id,
            inv.OrderId,
            inv.InvoiceNumber,
            inv.Currency,
            inv.TotalCents,
            inv.IssuedAt);
    }
}
