using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Auth;
using DomusMind.Contracts.Auth;

namespace DomusMind.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandHandler : ICommandHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly IAuthUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IRefreshTokenService _refreshTokens;

    public ChangePasswordCommandHandler(
        IAuthUserRepository users,
        IPasswordHasher hasher,
        IRefreshTokenService refreshTokens)
    {
        _users = users;
        _hasher = hasher;
        _refreshTokens = refreshTokens;
    }

    public async Task<ChangePasswordResponse> Handle(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _users.FindByIdAsync(command.UserId, cancellationToken);
        if (user is null)
            throw new AuthException(AuthErrorCode.UserNotFound, "User not found.");

        if (!_hasher.Verify(command.CurrentPassword, user.PasswordHash))
            throw new AuthException(AuthErrorCode.InvalidCurrentPassword, "Current password is incorrect.");

        if (command.CurrentPassword == command.NewPassword)
            throw new AuthException(AuthErrorCode.SamePassword, "New password must differ from the current password.");

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 8)
            throw new AuthException(AuthErrorCode.WeakPassword, "New password must be at least 8 characters.");

        var newHash = _hasher.Hash(command.NewPassword);
        await _users.UpdatePasswordHashAsync(command.UserId, newHash, cancellationToken);
        await _users.UpdateMustChangePasswordAsync(command.UserId, false, cancellationToken);

        // Revoke all refresh tokens so existing sessions are invalidated
        await _refreshTokens.RevokeAllForUserAsync(command.UserId, cancellationToken);

        await _users.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResponse(command.UserId);
    }
}
