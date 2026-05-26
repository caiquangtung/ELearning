using ELearning.Core.Common;
using FluentAssertions;

namespace ELearning.Application.UnitTests;

public class AuditSecurityTests
{
    [Fact]
    public void Audit_metadata_sanitizer_redacts_sensitive_keys()
    {
        var sanitized = AuditMetadataSanitizer.Sanitize(new Dictionary<string, string>
        {
            ["reason"] = "invalid",
            ["refreshToken"] = "secret-token",
            ["password"] = "plain",
            ["api_key"] = "key"
        });

        sanitized["reason"].Should().Be("invalid");
        sanitized["refreshToken"].Should().Be("[redacted]");
        sanitized["password"].Should().Be("[redacted]");
        sanitized["api_key"].Should().Be("[redacted]");
    }
}
