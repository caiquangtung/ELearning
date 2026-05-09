using FluentValidation;

namespace ELearning.Application.Features.Promotions.QuoteCheckout;

public sealed class QuoteCheckoutQueryValidator : AbstractValidator<QuoteCheckoutQuery>
{
    public QuoteCheckoutQueryValidator()
    {
        RuleFor(x => x.BuyerUserId).NotEmpty();
        RuleFor(x => x.Currency).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(i =>
        {
            i.RuleFor(x => x.ItemType).NotEmpty();
            i.RuleFor(x => x.ReferenceId).NotEmpty();
            i.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}

