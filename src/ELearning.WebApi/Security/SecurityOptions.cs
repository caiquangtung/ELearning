namespace ELearning.WebApi.Security;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";

    public bool EnableSecurityHeaders { get; init; } = true;
    public bool ValidateUnsafeRequestOrigins { get; init; } = true;
    public string ContentSecurityPolicy { get; init; } =
        "default-src 'self'; " +
        "base-uri 'self'; " +
        "frame-ancestors 'none'; " +
        "object-src 'none'; " +
        "img-src 'self' data: blob:; " +
        "font-src 'self' data:; " +
        "style-src 'self' 'unsafe-inline'; " +
        "script-src 'self'; " +
        "connect-src 'self'";
    public string ReferrerPolicy { get; init; } = "no-referrer";
    public string PermissionsPolicy { get; init; } =
        "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
    public int HstsMaxAgeDays { get; init; } = 365;
}
