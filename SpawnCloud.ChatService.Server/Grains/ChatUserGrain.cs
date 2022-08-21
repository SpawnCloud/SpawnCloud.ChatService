using Orleans;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Server.Grains;

public class ChatUserGrain : Grain, IChatUserGrain
{
    private readonly HashSet<Guid> _channels = new();
    
    public Guid UserId => this.GetPrimaryKey();
    
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
    public async Task SendMessage(ChatMessage message)
    {
        if (!_channels.Contains(message.ChannelId))
            throw new InvalidOperationException("Cannot send message to channel that user does not belong to.");

        message.SenderUserId = UserId;
        message.SendDate = DateTime.UtcNow;
        
        var channelGrain = GrainFactory.GetGrain<IChatChannelGrain>(message.ChannelId);
        await channelGrain.SendMessage(this, message);
    }
}