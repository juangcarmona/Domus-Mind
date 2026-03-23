using DomusMind.Application.Abstractions.Messaging;
using DomusMind.Contracts.Setup;

namespace DomusMind.Application.Features.Setup.InitializeSystem;

public sealed record InitializeSystemCommand(
    string Email,
    string Password,
    string? DisplayName) : ICommand<InitializeSystemResponse>;
