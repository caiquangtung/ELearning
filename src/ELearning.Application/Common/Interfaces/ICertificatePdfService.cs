using ELearning.Domain.Aggregates.CertificateAggregate;

namespace ELearning.Application.Common.Interfaces;

public interface ICertificatePdfService
{
    byte[] Generate(Certificate certificate);
}
