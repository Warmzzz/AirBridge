using AirBridge.Domain.Enums;

namespace AirBridge.Domain.Exceptions;

public sealed class ReceiverException : DomainException
{
    public ReceiverStatus StatusAtFailure { get; }
 
    public ReceiverException(string message, ReceiverStatus statusAtFailure)
        : base(message)
    {
        StatusAtFailure = statusAtFailure;
    }
 
    public ReceiverException(string message, ReceiverStatus statusAtFailure, Exception inner)
        : base(message, inner)
    {
        StatusAtFailure = statusAtFailure;
    }
}
