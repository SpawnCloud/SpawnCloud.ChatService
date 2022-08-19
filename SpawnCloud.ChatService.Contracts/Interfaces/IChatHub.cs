namespace SpawnCloud.ChatService.Contracts.Interfaces;

public interface IChatHub
{
    Task SendMessage(Guid channelId, string message);

    Task<bool> JoinChannel(Guid channelId);
}