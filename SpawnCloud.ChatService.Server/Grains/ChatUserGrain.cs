using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Server.Grains;

[Serializable]
internal class ChatUserState
{
    public const string PersistenceStateName = "ChatUser";
    
    public HashSet<Guid> Channels = new();
}

internal class ChatUserGrain : Grain, IChatUserGrain
{
    private readonly ILogger<ChatUserGrain> _logger;
    private readonly IPersistentState<ChatUserState> _state;
    
    public Guid UserId => this.GetPrimaryKey();

    public ChatUserGrain(ILogger<ChatUserGrain> logger,
        [PersistentState(ChatUserState.PersistenceStateName, Constants.ChatGrainStorage)]
        IPersistentState<ChatUserState> state)
    {
        _logger = logger;
        _state = state;
    }
    
    public async Task<ChannelDescription[]> ListChannels()
    {
        var descriptions = new List<ChannelDescription>();
        foreach (var channelId in _state.State.Channels)
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
            _state.State.Channels.Add(channelId);
            await _state.WriteStateAsync();
        }
        
        return result;
    }

    public async Task LeaveChannel(Guid channelId)
    {
        if (_state.State.Channels.Contains(channelId))
        {
            var channelGrain = GrainFactory.GetGrain<IChatChannelGrain>(channelId);
            await channelGrain.LeaveChannel(this);
            _state.State.Channels.Remove(channelId);
            await _state.WriteStateAsync();
        }
    }

    public async Task SendMessage(ChatMessage message)
    {
        if (!_state.State.Channels.Contains(message.ChannelId))
            throw new InvalidOperationException("Cannot send message to channel that user does not belong to.");

        message.SenderUserId = UserId;
        message.SendDate = DateTime.UtcNow;
        
        var channelGrain = GrainFactory.GetGrain<IChatChannelGrain>(message.ChannelId);
        await channelGrain.SendMessage(this, message);
    }
}