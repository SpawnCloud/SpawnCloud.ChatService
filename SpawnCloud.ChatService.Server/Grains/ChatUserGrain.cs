using Orleans;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Server.Grains;

public class ChatUserGrain : Grain, IChatUserGrain
{
    private readonly HashSet<Guid> _channels = new();
    
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

    public async Task<bool> JoinChannel(Guid channelId)
    {
        var channelGrain = GrainFactory.GetGrain<IChatChannelGrain>(channelId);
        var result = await channelGrain.JoinChannel(this);
        if (result)
        {
            _channels.Add(channelId);
        }
        
        return result;
    }

    public async Task SendMessage(Guid channelId, string message)
    {
        if (!_channels.Contains(channelId))
            throw new InvalidOperationException("Cannot send message to channel that user does not belong to.");
        
        var channelGrain = GrainFactory.GetGrain<IChatChannelGrain>(channelId);
        await channelGrain.SendMessage(this, message);
    }
}