using Orleans;
using SpawnCloud.ChatService.Shared.Contracts;
using SpawnCloud.ChatService.Shared.Grains;

namespace SpawnCloud.ChatService.Server.Grains;

public class ChatUserGrain : Grain, IChatUserGrain
{
    private readonly List<Guid> _channels = new();
    
    public async Task<ChannelDescription[]> ListChannels()
    {
        var descriptions = new List<ChannelDescription>();
        foreach (var channelId in _channels)
        {
            var chatChannelGrain = GrainFactory.GetGrain<IChatChannelGrain>(channelId);
            var channelDescription = await chatChannelGrain.GetDescription();
            descriptions.Add(channelDescription);
        }
        return descriptions.ToArray();
    }
}