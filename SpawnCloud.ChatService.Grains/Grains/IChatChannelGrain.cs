using Orleans;
using SpawnCloud.ChatService.Contracts.Models;

namespace SpawnCloud.ChatService.Grains;

public interface IChatChannelGrain : IGrainWithGuidKey
{
    Task InitializeChannel(ChannelSettings settings);
    
    Task<ChannelDescription> GetDescription();
    
    Task<bool> JoinChannel(IChatUserGrain chatUserGrain);

    Task LeaveChannel(IChatUserGrain chatUserGrain);
    
    Task SendMessage(IChatUserGrain chatUserGrain, ChatMessage message);
}