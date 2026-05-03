using ELearning.Application.Features.Promotions.QuoteCheckout;
using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CourseAggregate;
using ELearning.Domain.Aggregates.LicensePoolAggregate;
using ELearning.Domain.Aggregates.PromotionAggregate;
using ELearning.Domain.Aggregates.TrainingClassAggregate;
using FluentAssertions;
using NSubstitute;

namespace ELearning.Application.UnitTests;

public sealed class QuoteCheckoutTests
{
    [Fact]
    public void QuoteCheckoutQueryValidator_rejects_empty_buyer()
    {
        var v = new QuoteCheckoutQueryValidator();
        var result = v.Validate(new QuoteCheckoutQuery(Guid.Empty, null, "USD", [new QuoteCheckoutItem("Course", Guid.NewGuid(), 1)], null));
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task QuoteCheckout_applies_item_percent_off_only_to_target_item_types()
    {
        var couponRepo = Substitute.For<ICouponRepository>();
        var campaignRepo = Substitute.For<ICampaignRepository>();
        var redemptionRepo = Substitute.For<ICouponRedemptionRepository>();
        var courseRepo = Substitute.For<ICourseRepository>();
        var classRepo = Substitute.For<ITrainingClassRepository>();
        var poolRepo = Substitute.For<ILicensePoolRepository>();

        var buyerId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var classId = Guid.NewGuid();

        var campaign = Campaign.Create(
            "Test",
            CampaignScope.Global,
            null,
            DateTime.UtcNow.AddDays(-1),
            null);
        campaign.Activate(DateTime.UtcNow);
        campaign.AddItemPercentOffRule(20, ["Course"], DateTime.UtcNow);

        var coupon = Coupon.Create(campaign.Id, "SPRING20", null, 1);

        couponRepo.GetByCodeNormalizedAsync(Coupon.NormalizeCode("SPRING20"), Arg.Any<CancellationToken>())
            .Returns(coupon);
        campaignRepo.GetByIdWithRulesAndCouponsAsync(campaign.Id, Arg.Any<CancellationToken>())
            .Returns(campaign);
        redemptionRepo.CountForBuyerAsync(coupon.Id, buyerId, Arg.Any<CancellationToken>())
            .Returns(0);

        var course = Course.Create("C1", null);
        course.SetPrice(10_00, "USD"); // $10
        courseRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var tc = TrainingClass.Create(Guid.NewGuid(), "T1", 10);
        tc.SetPrice(5_00, "USD"); // $5
        classRepo.GetByIdAsync(classId, Arg.Any<CancellationToken>()).Returns(tc);

        poolRepo.GetByIdWithAssignmentsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((LicensePool?)null);

        var handler = new QuoteCheckoutQueryHandler(
            couponRepo,
            campaignRepo,
            redemptionRepo,
            courseRepo,
            classRepo,
            poolRepo);

        var q = new QuoteCheckoutQuery(
            buyerId,
            null,
            "USD",
            [new QuoteCheckoutItem("Course", courseId, 1), new QuoteCheckoutItem("TrainingClass", classId, 1)],
            "SPRING20");

        var result = await handler.Handle(q, default);
        result.IsSuccess.Should().BeTrue();
        result.Value.SubtotalCents.Should().Be(15_00);
        result.Value.DiscountCents.Should().Be(2_00); // 20% of 10.00
        result.Value.TotalCents.Should().Be(13_00);
    }

    [Fact]
    public async Task QuoteCheckout_rejects_org_mismatch_for_org_scoped_campaign()
    {
        var couponRepo = Substitute.For<ICouponRepository>();
        var campaignRepo = Substitute.For<ICampaignRepository>();
        var redemptionRepo = Substitute.For<ICouponRedemptionRepository>();
        var courseRepo = Substitute.For<ICourseRepository>();
        var classRepo = Substitute.For<ITrainingClassRepository>();
        var poolRepo = Substitute.For<ILicensePoolRepository>();

        var buyerId = Guid.NewGuid();
        var orgA = Guid.NewGuid();
        var orgB = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var campaign = Campaign.Create(
            "OrgPromo",
            CampaignScope.Organization,
            orgA,
            DateTime.UtcNow.AddDays(-1),
            null);
        campaign.Activate(DateTime.UtcNow);
        campaign.AddItemPercentOffRule(10, ["Course"], DateTime.UtcNow);

        var coupon = Coupon.Create(campaign.Id, "ORG10", null, 1);

        couponRepo.GetByCodeNormalizedAsync(Coupon.NormalizeCode("ORG10"), Arg.Any<CancellationToken>())
            .Returns(coupon);
        campaignRepo.GetByIdWithRulesAndCouponsAsync(campaign.Id, Arg.Any<CancellationToken>())
            .Returns(campaign);
        redemptionRepo.CountForBuyerAsync(coupon.Id, buyerId, Arg.Any<CancellationToken>())
            .Returns(0);

        var course = Course.Create("C1", null);
        course.SetPrice(10_00, "USD");
        courseRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var handler = new QuoteCheckoutQueryHandler(
            couponRepo,
            campaignRepo,
            redemptionRepo,
            courseRepo,
            classRepo,
            poolRepo);

        var q = new QuoteCheckoutQuery(
            buyerId,
            orgB,
            "USD",
            [new QuoteCheckoutItem("Course", courseId, 1)],
            "ORG10");

        var result = await handler.Handle(q, default);
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("Conflict");
    }

    [Fact]
    public async Task QuoteCheckout_rejects_when_per_buyer_limit_reached()
    {
        var couponRepo = Substitute.For<ICouponRepository>();
        var campaignRepo = Substitute.For<ICampaignRepository>();
        var redemptionRepo = Substitute.For<ICouponRedemptionRepository>();
        var courseRepo = Substitute.For<ICourseRepository>();
        var classRepo = Substitute.For<ITrainingClassRepository>();
        var poolRepo = Substitute.For<ILicensePoolRepository>();

        var buyerId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var campaign = Campaign.Create(
            "Promo",
            CampaignScope.Global,
            null,
            DateTime.UtcNow.AddDays(-1),
            null);
        campaign.Activate(DateTime.UtcNow);
        campaign.AddItemPercentOffRule(10, ["Course"], DateTime.UtcNow);

        var coupon = Coupon.Create(campaign.Id, "ONCE", null, 1);

        couponRepo.GetByCodeNormalizedAsync(Coupon.NormalizeCode("ONCE"), Arg.Any<CancellationToken>())
            .Returns(coupon);
        campaignRepo.GetByIdWithRulesAndCouponsAsync(campaign.Id, Arg.Any<CancellationToken>())
            .Returns(campaign);
        redemptionRepo.CountForBuyerAsync(coupon.Id, buyerId, Arg.Any<CancellationToken>())
            .Returns(1);

        var course = Course.Create("C1", null);
        course.SetPrice(10_00, "USD");
        courseRepo.GetByIdAsync(courseId, Arg.Any<CancellationToken>()).Returns(course);

        var handler = new QuoteCheckoutQueryHandler(
            couponRepo,
            campaignRepo,
            redemptionRepo,
            courseRepo,
            classRepo,
            poolRepo);

        var q = new QuoteCheckoutQuery(
            buyerId,
            null,
            "USD",
            [new QuoteCheckoutItem("Course", courseId, 1)],
            "ONCE");

        var result = await handler.Handle(q, default);
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Contain("Conflict");
    }
}

