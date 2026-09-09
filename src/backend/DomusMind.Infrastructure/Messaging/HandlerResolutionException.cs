namespace DomusMind.Infrastructure.Messaging;

public sealed class HandlerResolutionException : Exception
{
    public string HandlerType { get; }

    public HandlerResolutionException(string handlerType, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        HandlerType = handlerType;
    }
}
