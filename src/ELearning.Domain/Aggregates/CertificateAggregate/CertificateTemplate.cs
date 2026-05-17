using ELearning.Domain.Exceptions;
using ELearning.Domain.Shared;

namespace ELearning.Domain.Aggregates.CertificateAggregate;

public sealed class CertificateTemplate : AuditableAggregateRoot
{
    private CertificateTemplate() { }

    public string Name { get; private set; } = default!;
    public string HtmlTemplate { get; private set; } = default!;
    public bool IsDefault { get; private set; }

    public static CertificateTemplate Create(string name, string htmlTemplate, bool isDefault)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Template name is required.");
        if (string.IsNullOrWhiteSpace(htmlTemplate))
            throw new DomainException("Template HTML is required.");

        return new CertificateTemplate
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            HtmlTemplate = htmlTemplate.Trim(),
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };
    }
}
