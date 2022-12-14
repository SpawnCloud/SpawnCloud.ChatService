using Orleans;
using SpawnCloud.ChatService.Contracts.Models;

namespace SpawnCloud.ChatService.Grains;

public interface IChatUserGrain : IGrainWithGuidKey
{
    Task<ChannelDescription[]> ListChannels();

    Task<bool> JoinChannel(Guid channelId);

    Task LeaveChannel(Guid channelId);

    Task SendMessage(ChatMessage message);
}