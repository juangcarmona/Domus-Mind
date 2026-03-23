using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Application.Abstractions.Security;
using DomusMind.Application.Abstractions.System;
using DomusMind.Contracts.Setup;

namespace DomusMind.Application.Features.Setup.InitializeSystem;

public sealed class InitializeSystemCommandHandler
    : ICommandHandler<InitializeSystemCommand, InitializeSystemResponse>
{
    private readonly ISystemInitializationState _state;
    private readonly IAuthUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public InitializeSystemCommandHandler(
        ISystemInitializationState state,
        IAuthUserRepository users,
        IPasswordHasher hasher)
    {
        _state = state;
        _users = users;
        _hasher = hasher;
    }

    public async Task<InitializeSystemResponse> Handle(
        InitializeSystemCommand command,
        CancellationToken cancellationToken)
    {
        if (await _state.IsInitializedAsync(cancellationToken))
            throw new SetupException(SetupErrorCode.AlreadyInitialized, "The system has already been initialized.");

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
            throw new SetupException(SetupErrorCode.WeakPassword, "Password must be at least 8 characters.");

        var email = command.Email.Trim().ToLowerInvariant();

        var existing = await _users.FindByEmailAsync(email, cancellationToken);
        if (existing is not null)
            throw new SetupException(SetupErrorCode.EmailAlreadyTaken, "Email address is already registered.");

        var user = new AuthUserRecord(Guid.NewGuid(), email, _hasher.Hash(command.Password));

        await _users.AddAsync(user, cancellationToken);
        await _users.SaveChangesAsync(cancellationToken);
        await _state.MarkInitializedAsync(cancellationToken);

        return new InitializeSystemResponse(user.UserId, user.Email);
    }
}
