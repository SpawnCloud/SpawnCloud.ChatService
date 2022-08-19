using Orleans;
using SpawnCloud.ChatService.Contracts.Models;

namespace SpawnCloud.ChatService.Grains;

public interface IChatUserGrain : IGrainWithGuidKey
{
    Task<ChannelDescription[]> ListChannels();
}