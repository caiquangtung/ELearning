using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.PromotionAggregate;

public sealed class Campaign : AuditableAggregateRoot
{
    private readonly List<PromotionRule> _rules = [];
    private readonly List<Coupon> _coupons = [];

    private Campaign() { }

    public string Name { get; private set; } = default!;
    public CampaignScope Scope { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public CampaignStatus Status { get; private set; }

    public DateTime StartUtc { get; private set; }
    public DateTime? EndUtc { get; private set; }

    public IReadOnlyList<PromotionRule> Rules => _rules.AsReadOnly();
    public IReadOnlyList<Coupon> Coupons => _coupons.AsReadOnly();

    public static Campaign Create(string name, CampaignScope scope, Guid? organizationId, DateTime startUtc, DateTime? endUtc)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Campaign name is required.");
        if (startUtc == default) throw new DomainException("StartUtc is required.");
        if (endUtc is not null && endUtc.Value <= startUtc) throw new DomainException("EndUtc must be after StartUtc.");

        if (scope == CampaignScope.Organization && organizationId is null)
            throw new DomainException("OrganizationId is required for organization-scoped campaigns.");

        if (scope == CampaignScope.Global && organizationId is not null)
            throw new DomainException("OrganizationId must be null for global campaigns.");

        return new Campaign
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Scope = scope,
            OrganizationId = organizationId,
            Status = CampaignStatus.Draft,
            StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc),
            EndUtc = endUtc is null ? null : DateTime.SpecifyKind(endUtc.Value, DateTimeKind.Utc),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate(DateTime utcNow)
    {
        if (Status == CampaignStatus.Expired) throw new DomainException("Expired campaigns cannot be activated.");
        if (utcNow < StartUtc) throw new DomainException("Campaign cannot be activated before StartUtc.");
        if (EndUtc is not null && utcNow >= EndUtc.Value) throw new DomainException("Campaign has already ended.");

        Status = CampaignStatus.Active;
        UpdatedAt = utcNow;
    }

    public void Pause(DateTime utcNow)
    {
        if (Status != CampaignStatus.Active) throw new DomainException("Only active campaigns can be paused.");
        Status = CampaignStatus.Paused;
        UpdatedAt = utcNow;
    }

    public void SetWindow(DateTime startUtc, DateTime? endUtc, DateTime utcNow)
    {
        if (startUtc == default) throw new DomainException("StartUtc is required.");
        if (endUtc is not null && endUtc.Value <= startUtc) throw new DomainException("EndUtc must be after StartUtc.");

        StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        EndUtc = endUtc is null ? null : DateTime.SpecifyKind(endUtc.Value, DateTimeKind.Utc);
        UpdatedAt = utcNow;
    }

    public PromotionRule AddItemPercentOffRule(int percentOff, IReadOnlyList<string> appliesToItemTypes, DateTime utcNow)
    {
        var rule = PromotionRule.CreateItemPercentOff(Id, percentOff, appliesToItemTypes);
        _rules.Add(rule);
        UpdatedAt = utcNow;
        return rule;
    }

    public Coupon AddCoupon(string code, DateTime? expiresUtc, int perBuyerMaxRedemptions, DateTime utcNow)
    {
        var normalized = Coupon.NormalizeCode(code);
        if (_coupons.Any(c => c.CodeNormalized.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Coupon code already exists in this campaign.");

        var coupon = Coupon.Create(Id, code, expiresUtc, perBuyerMaxRedemptions);
        _coupons.Add(coupon);
        UpdatedAt = utcNow;
        return coupon;
    }

    public bool IsEligibleFor(Guid? organizationId, DateTime utcNow)
    {
        if (Status != CampaignStatus.Active) return false;
        if (utcNow < StartUtc) return false;
        if (EndUtc is not null && utcNow >= EndUtc.Value) return false;

        return Scope switch
        {
            CampaignScope.Global => true,
            CampaignScope.Organization => OrganizationId == organizationId,
            _ => false
        };
    }
}

