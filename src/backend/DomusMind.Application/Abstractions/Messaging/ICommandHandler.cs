namespace DomusMind.Application.Abstractions.Messaging;

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> Handle(
        TCommand command,
        CancellationToken cancellationToken);
}
