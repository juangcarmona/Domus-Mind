namespace DomusMind.Application.Abstractions.Messaging;

public interface IQueryDispatcher
{
    Task<TResponse> Dispatch<TResponse>(
        IQuery<TResponse> query,
        CancellationToken cancellationToken = default);
}
