using DomusMind.Api.Controllers;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Contracts.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace DomusMind.Api.Tests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public void Health_ReturnsOk_WithStatusOk()
    {
        var sut = new AuthController(new StubCurrentUser(null, null));

        var result = sut.Health();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Me_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        var sut = new AuthController(new StubCurrentUser(null, null));

        var result = sut.Me();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void Me_WhenUserIdPresent_ReturnsOkWithMeResponse()
    {
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var sut = new AuthController(new StubCurrentUser(userId, email));

        var result = sut.Me();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var me = ok.Value.Should().BeOfType<MeResponse>().Subject;
        me.UserId.Should().Be(userId);
        me.Email.Should().Be(email);
    }

    [Fact]
    public void Me_WhenEmailIsNull_ReturnsOkWithNullEmail()
    {
        var userId = Guid.NewGuid();
        var sut = new AuthController(new StubCurrentUser(userId, null));

        var result = sut.Me();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        var me = ok.Value.Should().BeOfType<MeResponse>().Subject;
        me.UserId.Should().Be(userId);
        me.Email.Should().BeNull();
    }

    private sealed class StubCurrentUser : ICurrentUser
    {
        private readonly Guid? _userId;
        private readonly string? _email;

        public StubCurrentUser(Guid? userId, string? email)
        {
            _userId = userId;
            _email = email;
        }

        public Guid? UserId => _userId;
        public string? Email => _email;
    }
}
