using ELearning.Core.Abstractions;
using ELearning.Domain.Aggregates.CertificateAggregate;
using ELearning.Infrastructure.Persistence;
using ELearning.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ELearning.Infrastructure.Certificates;

public sealed class CertificateRepository(ApplicationDbContext context)
    : GenericRepository<Certificate>(context), ICertificateRepository
{
    public async Task<Certificate?> GetByVerificationCodeAsync(string verificationCode, CancellationToken ct = default)
        => await DbSet.FirstOrDefaultAsync(c => c.VerificationCode == verificationCode, ct);

    public async Task<bool> ExistsForCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default)
        => await DbSet.AnyAsync(c => c.UserId == userId && c.CourseId == courseId, ct);
}
