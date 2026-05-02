using System.Text.Json;
using ELearning.Domain.Aggregates.OrderAggregate;
using ELearning.Domain.Aggregates.PromotionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ELearning.Infrastructure.Persistence.Configurations;

public sealed class PromotionRuleConfiguration : IEntityTypeConfiguration<PromotionRule>
{
    public void Configure(EntityTypeBuilder<PromotionRule> builder)
    {
        builder.ToTable("promotion_rules");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.CampaignId).HasColumnName("campaign_id").IsRequired();
        builder.Property(r => r.RuleType).HasColumnName("rule_type").HasConversion<string>().IsRequired();
        builder.Property(r => r.PercentOff).HasColumnName("percent_off").IsRequired();

        builder.Property<List<OrderItemType>>("_appliesToItemTypes")
            .HasColumnName("applies_to_item_types")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<OrderItemType>>(v, (JsonSerializerOptions?)null) ?? new List<OrderItemType>())
            .Metadata.SetValueComparer(new ValueComparer<List<OrderItemType>>(
                (a, b) => (a ?? new List<OrderItemType>()).SequenceEqual(b ?? new List<OrderItemType>()),
                v => (v ?? new List<OrderItemType>()).Aggregate(0, (h, e) => HashCode.Combine(h, e.GetHashCode())),
                v => (v ?? new List<OrderItemType>()).ToList()));

        builder.HasIndex(r => r.CampaignId);
    }
}

