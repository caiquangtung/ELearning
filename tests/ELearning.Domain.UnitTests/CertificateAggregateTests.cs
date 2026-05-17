using ELearning.Domain.Aggregates.CertificateAggregate;
using ELearning.Domain.Exceptions;
using FluentAssertions;

namespace ELearning.Domain.UnitTests;

public class CertificateAggregateTests
{
    [Fact]
    public void Issue_creates_verifiable_certificate_when_completion_rules_are_met()
    {
        var certificate = Certificate.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Jane Learner",
            "Domain Driven Design",
            90m,
            100m,
            true);

        certificate.Status.Should().Be(CertificateStatus.Issued);
        certificate.CertificateNumber.Should().StartWith("CERT-");
        certificate.VerificationCode.Should().NotBeNullOrWhiteSpace();
        certificate.IsVerifiable(DateTime.UtcNow).Should().BeTrue();
    }

    [Theory]
    [InlineData(79.99, 100, true)]
    [InlineData(80, 99.99, true)]
    [InlineData(80, 100, false)]
    public void Issue_rejects_incomplete_coursework(decimal attendance, decimal progress, bool quizPassed)
    {
        var act = () => Certificate.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Jane Learner",
            "Domain Driven Design",
            attendance,
            progress,
            quizPassed);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Revoke_makes_certificate_non_verifiable()
    {
        var certificate = Certificate.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Jane Learner",
            "Domain Driven Design",
            80m,
            100m,
            true);

        certificate.Revoke("Issued in error");

        certificate.Status.Should().Be(CertificateStatus.Revoked);
        certificate.IsVerifiable(DateTime.UtcNow).Should().BeFalse();
    }
}
