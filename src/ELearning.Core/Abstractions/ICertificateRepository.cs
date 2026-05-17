using ELearning.Domain.Aggregates.CertificateAggregate;

namespace ELearning.Core.Abstractions;

public interface ICertificateRepository : IRepository<Certificate>
{
    Task<Certificate?> GetByVerificationCodeAsync(string verificationCode, CancellationToken ct = default);
    Task<bool> ExistsForCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default);
}
