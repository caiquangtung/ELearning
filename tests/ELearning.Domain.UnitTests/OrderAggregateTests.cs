using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class OrderAggregateTests
{
    [Fact]
    public void CreateDraft_sets_defaults()
    {
        var buyerId = Guid.NewGuid();
        var order = Order.CreateDraft(buyerId, null, "usd");

        order.BuyerUserId.Should().Be(buyerId);
        order.OrganizationId.Should().BeNull();
        order.Status.Should().Be(OrderStatus.Draft);
        order.Currency.Should().Be("USD");
        order.TotalCents.Should().Be(0);
        order.Items.Should().BeEmpty();
    }

    [Fact]
    public void AddItem_recalculates_totals()
    {
        var order = Order.CreateDraft(Guid.NewGuid(), null, "USD");
        order.AddItem(OrderItemType.Course, Guid.NewGuid(), 2, 1500);

        order.SubtotalCents.Should().Be(3000);
        order.TotalCents.Should().Be(3000);
        order.Items.Should().ContainSingle();
    }

    [Fact]
    public void ApplyManualDiscount_cannot_exceed_subtotal()
    {
        var order = Order.CreateDraft(Guid.NewGuid(), null, "USD");
        order.AddItem(OrderItemType.Course, Guid.NewGuid(), 1, 1000);

        var act = () => order.ApplyManualDiscount(1001);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SubmitForPayment_requires_items_and_positive_total()
    {
        var order = Order.CreateDraft(Guid.NewGuid(), null, "USD");
        var act = () => order.SubmitForPayment();
        act.Should().Throw<DomainException>();

        order.AddItem(OrderItemType.Course, Guid.NewGuid(), 1, 1000);
        order.ApplyManualDiscount(1000);

        var act2 = () => order.SubmitForPayment();
        act2.Should().Throw<DomainException>();
    }

    [Fact]
    public void SubmitForPayment_transitions_to_pending_payment()
    {
        var order = Order.CreateDraft(Guid.NewGuid(), null, "USD");
        order.AddItem(OrderItemType.Course, Guid.NewGuid(), 1, 1000);

        order.SubmitForPayment();

        order.Status.Should().Be(OrderStatus.PendingPayment);
    }

    [Fact]
    public void MarkPaid_requires_pending_payment()
    {
        var order = Order.CreateDraft(Guid.NewGuid(), null, "USD");
        var act = () => order.MarkPaid();
        act.Should().Throw<DomainException>();

        order.AddItem(OrderItemType.Course, Guid.NewGuid(), 1, 1000);
        order.SubmitForPayment();
        order.MarkPaid();

        order.Status.Should().Be(OrderStatus.Paid);
    }

    [Fact]
    public void Cancel_disallows_paid()
    {
        var order = Order.CreateDraft(Guid.NewGuid(), null, "USD");
        order.AddItem(OrderItemType.Course, Guid.NewGuid(), 1, 1000);
        order.SubmitForPayment();
        order.MarkPaid();

        var act = () => order.Cancel("nope");
        act.Should().Throw<DomainException>();
    }
}

