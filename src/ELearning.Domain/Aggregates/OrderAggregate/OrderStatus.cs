namespace ELearning.Domain.Aggregates.OrderAggregate;

public enum OrderStatus
{
    Draft = 0,
    PendingPayment = 1,
    Paid = 2,
    Cancelled = 3
}

