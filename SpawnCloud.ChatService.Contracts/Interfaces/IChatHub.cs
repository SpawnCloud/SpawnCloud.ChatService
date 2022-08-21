using SpawnCloud.ChatService.Contracts.Models;

namespace SpawnCloud.ChatService.Contracts.Interfaces;

public interface IChatHub
{
    Task SendMessage(ChatMessage message);

    Task<bool> JoinChannel(Guid channelId);
}