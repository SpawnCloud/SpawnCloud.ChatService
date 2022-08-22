using System.Runtime.Serialization;

namespace SpawnCloud.ChatService.Contracts.Exceptions;

[Serializable]
public class ChannelDoesNotExistException : Exception
{
    public ChannelDoesNotExistException()
    {
    }

    public ChannelDoesNotExistException(string message) : base(message)
    {
    }

    public ChannelDoesNotExistException(string message, Exception inner) : base(message, inner)
    {
    }

    protected ChannelDoesNotExistException(
        SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}