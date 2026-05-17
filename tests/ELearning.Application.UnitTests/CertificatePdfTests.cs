using System.Text;
using ELearning.Domain.Aggregates.CertificateAggregate;
using ELearning.Infrastructure.Certificates;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class CertificatePdfTests
{
    [Fact]
    public void Generate_returns_pdf_bytes_with_certificate_content()
    {
        var certificate = Certificate.Issue(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            null,
            "Jane Learner",
            "Domain Driven Design",
            90m,
            100m,
            true);

        var pdf = new SimpleCertificatePdfService().Generate(certificate);
        var text = Encoding.ASCII.GetString(pdf);

        text.Should().StartWith("%PDF-1.4");
        text.Should().Contain("CERTIFICATE OF COMPLETION");
        text.Should().Contain("Jane Learner");
        text.Should().Contain(certificate.VerificationCode);
    }
}
