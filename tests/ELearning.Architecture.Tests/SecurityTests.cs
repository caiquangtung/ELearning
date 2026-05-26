using ELearning.WebApi.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ELearning.Architecture.Tests;

public class SecurityTests
{
    [Fact]
    public void Cors_configuration_rejects_wildcard_origins()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => SecurityConfiguration.ParseAllowedOrigins("*"));
        Assert.Contains("wildcard", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Production_cors_configuration_requires_explicit_origins()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var env = new FakeWebHostEnvironment("Production");

        var ex = Assert.Throws<InvalidOperationException>(() => SecurityConfiguration.GetAllowedOrigins(config, env));
        Assert.Contains("Cors:AllowedOrigins", ex.Message);
    }

    [Fact]
    public async Task Security_headers_middleware_adds_expected_headers()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(
            _ => Task.CompletedTask,
            Options.Create(new SecurityOptions()),
            new FakeWebHostEnvironment("Production"));

        await middleware.InvokeAsync(context);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
        Assert.True(context.Response.Headers.ContainsKey("Content-Security-Policy"));
        Assert.True(context.Response.Headers.ContainsKey("Strict-Transport-Security"));
    }

    [Fact]
    public async Task Unsafe_request_origin_middleware_rejects_cross_origin_posts()
    {
        var called = false;
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers.Origin = "https://evil.example.com";

        var middleware = new UnsafeRequestOriginMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            Options.Create(new SecurityOptions()),
            Config("https://app.example.com"),
            new FakeWebHostEnvironment("Production"));

        await middleware.InvokeAsync(context);

        Assert.False(called);
        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task Unsafe_request_origin_middleware_allows_configured_origin_posts()
    {
        var called = false;
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Headers.Origin = "https://app.example.com";

        var middleware = new UnsafeRequestOriginMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            Options.Create(new SecurityOptions()),
            Config("https://app.example.com"),
            new FakeWebHostEnvironment("Production"));

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public async Task Unsafe_request_origin_middleware_allows_same_origin_posts()
    {
        var called = false;
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("api.example.com");
        context.Request.Headers.Origin = "https://api.example.com";

        var middleware = new UnsafeRequestOriginMiddleware(
            _ =>
            {
                called = true;
                return Task.CompletedTask;
            },
            Options.Create(new SecurityOptions()),
            Config("https://app.example.com"),
            new FakeWebHostEnvironment("Production"));

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }

    [Fact]
    public void Webhook_secret_validator_accepts_only_matching_secret()
    {
        Assert.True(WebhookSecretValidator.IsValid("expected-secret", "expected-secret"));
        Assert.False(WebhookSecretValidator.IsValid("expected-secret", "wrong-secret"));
        Assert.False(WebhookSecretValidator.IsValid("expected-secret", null));
    }

    private static IConfiguration Config(string origins) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Cors:AllowedOrigins"] = origins })
            .Build();

    private sealed class FakeWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "ELearning.Tests";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
