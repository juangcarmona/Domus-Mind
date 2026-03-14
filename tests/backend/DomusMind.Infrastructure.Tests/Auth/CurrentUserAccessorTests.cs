using System.Security.Claims;
using DomusMind.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace DomusMind.Infrastructure.Tests.Auth;

public sealed class CurrentUserAccessorTests
{
    [Fact]
    public void UserId_WhenNoHttpContext_ReturnsNull()
    {
        var accessor = new StubHttpContextAccessor(null);
        var sut = new CurrentUserAccessor(accessor);

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void Email_WhenNoHttpContext_ReturnsNull()
    {
        var accessor = new StubHttpContextAccessor(null);
        var sut = new CurrentUserAccessor(accessor);

        sut.Email.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenNoNameIdentifierClaim_ReturnsNull()
    {
        var context = BuildContext(new ClaimsIdentity());
        var accessor = new StubHttpContextAccessor(context);
        var sut = new CurrentUserAccessor(accessor);

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenNameIdentifierIsValidGuid_ReturnsGuid()
    {
        var userId = Guid.NewGuid();
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, authenticationType: "test");
        var context = BuildContext(identity);
        var accessor = new StubHttpContextAccessor(context);
        var sut = new CurrentUserAccessor(accessor);

        sut.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WhenNameIdentifierIsNotGuid_ReturnsNull()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        }, authenticationType: "test");
        var context = BuildContext(identity);
        var accessor = new StubHttpContextAccessor(context);
        var sut = new CurrentUserAccessor(accessor);

        sut.UserId.Should().BeNull();
    }

    [Fact]
    public void Email_WhenEmailClaimPresent_ReturnsEmail()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Email, "user@example.com")
        }, authenticationType: "test");
        var context = BuildContext(identity);
        var accessor = new StubHttpContextAccessor(context);
        var sut = new CurrentUserAccessor(accessor);

        sut.Email.Should().Be("user@example.com");
    }

    [Fact]
    public void Email_WhenNoEmailClaim_ReturnsNull()
    {
        var context = BuildContext(new ClaimsIdentity());
        var accessor = new StubHttpContextAccessor(context);
        var sut = new CurrentUserAccessor(accessor);

        sut.Email.Should().BeNull();
    }

    private static DefaultHttpContext BuildContext(ClaimsIdentity identity)
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(identity);
        return context;
    }

    private sealed class StubHttpContextAccessor : IHttpContextAccessor
    {
        public StubHttpContextAccessor(HttpContext? context) => HttpContext = context;
        public HttpContext? HttpContext { get; set; }
    }
}
