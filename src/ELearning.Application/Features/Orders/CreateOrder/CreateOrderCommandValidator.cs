using FluentValidation;

namespace ELearning.Application.Features.Orders.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.BuyerUserId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty().Length(3);
        RuleFor(x => x.Items).NotNull().NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateOrderItemValidator());
        RuleFor(x => x.DiscountCents).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CouponCode).MaximumLength(64);
    }
}

public sealed class CreateOrderItemValidator : AbstractValidator<CreateOrderItem>
{
    private static readonly string[] AllowedTypes = ["Course", "TrainingClass", "LicensePool"];

    public CreateOrderItemValidator()
    {
        RuleFor(x => x.ItemType).NotEmpty().Must(t => AllowedTypes.Contains(t)).WithMessage("Invalid ItemType.");
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(100_000);
        RuleFor(x => x.UnitPriceCents).GreaterThanOrEqualTo(0);
    }
}

