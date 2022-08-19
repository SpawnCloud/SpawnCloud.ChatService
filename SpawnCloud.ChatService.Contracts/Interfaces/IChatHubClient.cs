namespace SpawnCloud.ChatService.Contracts.Interfaces;

public interface IChatHubClient
{
    Task ReceiveMessage(Guid userId, string message);

    Task UserJoinedChannel(Guid channelId, Guid userId);
}