using FluentValidation;

namespace ELearning.Application.Features.Orders.ListMyOrders;

public sealed class ListMyOrdersQueryValidator : AbstractValidator<ListMyOrdersQuery>
{
    public ListMyOrdersQueryValidator()
    {
        RuleFor(x => x.BuyerUserId).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.PageSize).GreaterThan(0).LessThanOrEqualTo(200);
    }
}
