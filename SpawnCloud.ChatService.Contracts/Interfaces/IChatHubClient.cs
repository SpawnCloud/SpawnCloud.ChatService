using SpawnCloud.ChatService.Contracts.Models;

namespace SpawnCloud.ChatService.Contracts.Interfaces;

public interface IChatHubClient
{
    Task ReceiveMessage(ChatMessage message);

    Task UserJoinedChannel(Guid channelId, Guid userId);
}