using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.PromotionAggregate;

public sealed class PromotionRule : Entity
{
    private readonly List<OrderItemType> _appliesToItemTypes = [];

    private PromotionRule() { }

    public Guid CampaignId { get; private set; }
    public PromotionRuleType RuleType { get; private set; }

    /// <summary>Percent off, 1..100.</summary>
    public int PercentOff { get; private set; }

    public IReadOnlyList<OrderItemType> AppliesToItemTypes => _appliesToItemTypes.AsReadOnly();

    public static PromotionRule CreateItemPercentOff(Guid campaignId, int percentOff, IReadOnlyList<string> appliesToItemTypes)
    {
        if (campaignId == Guid.Empty) throw new DomainException("CampaignId is required.");
        if (percentOff <= 0 || percentOff > 100) throw new DomainException("PercentOff must be between 1 and 100.");
        if (appliesToItemTypes.Count == 0) throw new DomainException("AppliesToItemTypes is required.");

        var parsed = new List<OrderItemType>();
        foreach (var raw in appliesToItemTypes)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            if (!Enum.TryParse<OrderItemType>(raw.Trim(), ignoreCase: true, out var t))
                throw new DomainException($"Invalid OrderItemType: {raw}");
            parsed.Add(t);
        }

        if (parsed.Count == 0) throw new DomainException("AppliesToItemTypes is required.");

        var rule = new PromotionRule
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            RuleType = PromotionRuleType.ItemPercentOff,
            PercentOff = percentOff
        };

        foreach (var t in parsed.Distinct())
            rule._appliesToItemTypes.Add(t);

        return rule;
    }
}

