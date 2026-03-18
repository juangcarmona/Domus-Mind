using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Auth;
using DomusMind.Contracts.Auth;

namespace DomusMind.Application.Features.Auth.Login;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly IAuthUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IAccessTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenService _refreshTokens;

    public LoginCommandHandler(
        IAuthUserRepository users,
        IPasswordHasher hasher,
        IAccessTokenGenerator tokenGenerator,
        IRefreshTokenService refreshTokens)
    {
        _users = users;
        _hasher = hasher;
        _tokenGenerator = tokenGenerator;
        _refreshTokens = refreshTokens;
    }

    public async Task<LoginResponse> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        var user = await _users.FindByEmailAsync(email, cancellationToken);
        if (user is null || !_hasher.Verify(command.Password, user.PasswordHash))
            throw new AuthException(AuthErrorCode.InvalidCredentials, "Invalid email or password.");

        var accessToken = _tokenGenerator.Generate(user.UserId, user.Email);
        var refreshToken = await _refreshTokens.CreateAsync(user.UserId, cancellationToken);

        return new LoginResponse(accessToken, refreshToken, user.UserId, user.Email, user.MustChangePassword);
    }
}
