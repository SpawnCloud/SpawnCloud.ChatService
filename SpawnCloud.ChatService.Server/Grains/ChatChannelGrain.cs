using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using SignalR.Orleans.Core;
using SpawnCloud.ChatService.Contracts.Exceptions;
using SpawnCloud.ChatService.Contracts.Interfaces;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;
using SpawnCloud.ChatService.Hubs;

namespace SpawnCloud.ChatService.Server.Grains;

[Serializable]
internal class ChatChannelState
{
    public const string PersistenceStateName = "ChatChannel";
    
    public bool IsCreated { get; set; }
    public string Name { get; set; } = string.Empty;
    public HashSet<Guid> Users { get; set; } = new();
    public Dictionary<Guid, ChatMessage> Messages { get; set; } = new();
}

internal class ChatChannelGrain : Grain, IChatChannelGrain
{
    private readonly ILogger<ChatChannelGrain> _logger;
    private HubContext<ChatHub> _hubContext = null!;

    private readonly IPersistentState<ChatChannelState> _channelState;

    public Guid ChannelId => this.GetPrimaryKey();

    public ChatChannelGrain(ILogger<ChatChannelGrain> logger,
        [PersistentState(ChatChannelState.PersistenceStateName, Constants.ChatGrainStorage)]
        IPersistentState<ChatChannelState> state)
    {
        _logger = logger;
        _channelState = state;
    }

    public override Task OnActivateAsync()
    {
        _hubContext = GrainFactory.GetHub<ChatHub>();
        return base.OnActivateAsync();
    }

    public async Task InitializeChannel(ChannelSettings settings)
    {
        if (_channelState.State.IsCreated) throw new InvalidOperationException("Channel has already been initialized.");
        
        _channelState.State.Name = settings.Name;
        _channelState.State.IsCreated = true;
        await _channelState.WriteStateAsync();
        
        _logger.LogInformation("Channel {ChannelId} created", ChannelId);
    }

    public Task<ChannelDescription> GetDescription()
    {
        CheckChannelCreated();
        
        var currentState = _channelState.State;
        return Task.FromResult(new ChannelDescription
        {
            Id = ChannelId,
            Name = currentState.Name,
            UserCount = currentState.Users.Count
        });
    }

    public async Task<bool> JoinChannel(IChatUserGrain chatUserGrain)
    {
        CheckChannelCreated();

        var userId = chatUserGrain.GetPrimaryKey();
        _channelState.State.Users.Add(userId);
        await _channelState.WriteStateAsync();

        await SendToAllUsers(nameof(IChatHubClient.UserJoinedChannel), ChannelId, userId);
        
        return true;
    }

    public async Task LeaveChannel(IChatUserGrain chatUserGrain)
    {
        CheckChannelCreated();
        
        var userId = chatUserGrain.GetPrimaryKey();
        if (_channelState.State.Users.Remove(userId))
        {
            await _channelState.WriteStateAsync();
            _logger.LogInformation("User {UserId} has left channel {ChannelId}", userId, ChannelId);
            
            if (_channelState.State.Users.Count <= 0)
            {
                _logger.LogInformation("Channel {ChannelId} is now empty and will be deactivated", ChannelId);
                DeactivateOnIdle();
            }
            else
            {
                await SendToAllUsers(nameof(IChatHubClient.UserLeftChannel), ChannelId, userId);
            }
        }
    }

    public async Task SendMessage(IChatUserGrain chatUserGrain, ChatMessage message)
    {
        CheckChannelCreated();
        
        if (!_channelState.State.Messages.ContainsKey(message.Id))
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("Channel {ChannelId} received message with ID {MessageId} from user {UserId} at {Timestamp}: {Body}", ChannelId, message.Id, message.SenderUserId, message.SendDate, message.Body);
            
            _channelState.State.Messages.Add(message.Id, message);
            await _channelState.WriteStateAsync(); // TODO: Should we be writing state on every message?
            await SendToAllUsers(nameof(IChatHubClient.ReceiveMessage), message);
        }
    }

    private void CheckChannelCreated()
    {
        if (!_channelState.State.IsCreated)
        {
            DeactivateOnIdle();
            throw new ChannelDoesNotExistException($"Channel {ChannelId} does not exist.");
        }
    }
    
    private async Task SendToAllUsers(string methodName, params object?[] args)
    {
        var users = _channelState.State.Users.Select(id => id.ToString()).ToArray();
        await _hubContext.SendToUsers(users, methodName, args);
    }
}