using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Auth;
using DomusMind.Application.Features.Auth.Login;
using FluentAssertions;

namespace DomusMind.Application.Tests.Features.Auth;

public sealed class LoginCommandHandlerTests
{
    private static readonly AuthUserRecord TestUser = new(
        Guid.NewGuid(),
        "user@example.com",
        "HASHED:CorrectPassword1!");

    private static LoginCommandHandler BuildHandler(
        InMemoryAuthUserRepository? users = null,
        StubRefreshTokenService? refreshTokens = null)
    {
        var repo = users ?? new InMemoryAuthUserRepository([TestUser]);
        return new LoginCommandHandler(
            repo,
            new StubPasswordHasher(),
            new StubAccessTokenGenerator(),
            refreshTokens ?? new StubRefreshTokenService());
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsLoginResponse()
    {
        var handler = BuildHandler();

        var result = await handler.Handle(
            new LoginCommand("user@example.com", "CorrectPassword1!"),
            CancellationToken.None);

        result.UserId.Should().Be(TestUser.UserId);
        result.Email.Should().Be(TestUser.Email);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsAuthException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new LoginCommand("user@example.com", "WrongPassword!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AuthException>()
            .Where(e => e.Code == AuthErrorCode.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsAuthException()
    {
        var handler = BuildHandler();

        var act = () => handler.Handle(
            new LoginCommand("nobody@example.com", "CorrectPassword1!"),
            CancellationToken.None);

        await act.Should().ThrowAsync<AuthException>()
            .Where(e => e.Code == AuthErrorCode.InvalidCredentials);
    }

    [Fact]
    public async Task Handle_NormalizesEmailBeforeLookup()
    {
        var handler = BuildHandler();

        // User is stored as lowercase; request uses mixed case - should still authenticate.
        var act = () => handler.Handle(
            new LoginCommand("USER@EXAMPLE.COM", "CorrectPassword1!"),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
