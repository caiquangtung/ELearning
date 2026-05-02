using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.PromotionAggregate;

public sealed class Coupon : AuditableAggregateRoot
{
    private Coupon() { }

    public Guid CampaignId { get; private set; }
    public string Code { get; private set; } = default!;
    public string CodeNormalized { get; private set; } = default!;
    public CouponStatus Status { get; private set; }
    public DateTime? ExpiresUtc { get; private set; }

    /// <summary>MVP: per-buyer redemption cap; 1 means single-use per buyer.</summary>
    public int PerBuyerMaxRedemptions { get; private set; }

    public static Coupon Create(Guid campaignId, string code, DateTime? expiresUtc, int perBuyerMaxRedemptions)
    {
        if (campaignId == Guid.Empty) throw new DomainException("CampaignId is required.");
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Coupon code is required.");
        if (perBuyerMaxRedemptions <= 0) throw new DomainException("PerBuyerMaxRedemptions must be positive.");

        if (expiresUtc is not null && expiresUtc.Value == default)
            throw new DomainException("ExpiresUtc is invalid.");

        var normalized = NormalizeCode(code);

        return new Coupon
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Code = code.Trim(),
            CodeNormalized = normalized,
            Status = CouponStatus.Active,
            ExpiresUtc = expiresUtc is null ? null : DateTime.SpecifyKind(expiresUtc.Value, DateTimeKind.Utc),
            PerBuyerMaxRedemptions = perBuyerMaxRedemptions,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Disable(DateTime utcNow)
    {
        Status = CouponStatus.Disabled;
        UpdatedAt = utcNow;
    }

    public bool IsValidAt(DateTime utcNow)
    {
        if (Status != CouponStatus.Active) return false;
        if (ExpiresUtc is not null && utcNow >= ExpiresUtc.Value) return false;
        return true;
    }

    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Coupon code is required.");

        var normalized = new string(code.Trim().Where(c => !char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();
        if (normalized.Length < 3) throw new DomainException("Coupon code is too short.");
        if (normalized.Length > 64) throw new DomainException("Coupon code is too long.");
        return normalized;
    }
}

