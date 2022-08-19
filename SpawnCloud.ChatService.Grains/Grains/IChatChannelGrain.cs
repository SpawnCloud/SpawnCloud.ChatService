using Orleans;
using SpawnCloud.ChatService.Contracts.Models;

namespace SpawnCloud.ChatService.Grains;

public interface IChatChannelGrain : IGrainWithGuidKey
{
    Task<ChannelDescription> GetDescription();
}