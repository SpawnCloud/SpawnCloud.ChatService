namespace SpawnCloud.ChatService.Contracts.Interfaces;

public interface IChatHub
{
    Task SendMessage(Guid userId, string message);

    Task JoinChannel(Guid channelId);
}