using Orleans;
using SpawnCloud.ChatService.Shared.Contracts;

namespace SpawnCloud.ChatService.Shared.Grains;

public interface IChatUserGrain : IGrainWithGuidKey
{
    Task<ChannelDescription[]> ListChannels();
}