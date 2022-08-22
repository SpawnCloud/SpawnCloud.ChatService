using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using SignalR.Orleans.Core;
using SpawnCloud.ChatService.Contracts.Interfaces;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;
using SpawnCloud.ChatService.Hubs;

namespace SpawnCloud.ChatService.Server.Grains;

public class ChatChannelGrain : Grain, IChatChannelGrain
{
    private readonly ILogger<ChatChannelGrain> _logger;
    private readonly HashSet<Guid> _users = new();
    private readonly Dictionary<Guid, ChatMessage> _messages = new();
    private HubContext<ChatHub> _hubContext = null!;

    public Guid ChannelId => this.GetPrimaryKey();

    public ChatChannelGrain(ILogger<ChatChannelGrain> logger)
    {
        _logger = logger;
    }

    public override Task OnActivateAsync()
    {
        _hubContext = GrainFactory.GetHub<ChatHub>();
        return base.OnActivateAsync();
    }

    public Task<ChannelDescription> GetDescription()
    {
        return Task.FromResult(new ChannelDescription
        {
            Id = ChannelId,
            Name = "Test"
        });
    }

    public async Task SendMessage(IChatUserGrain chatUserGrain, ChatMessage message)
    {
        if (!_messages.ContainsKey(message.Id))
        {
            _messages.Add(message.Id, message);
            await SendToAllUsers(nameof(IChatHubClient.ReceiveMessage), message);
        }
    }

    public async Task<bool> JoinChannel(IChatUserGrain chatUserGrain)
    {
        var userId = chatUserGrain.GetPrimaryKey();
        _users.Add(userId);

        await SendToAllUsers(nameof(IChatHubClient.UserJoinedChannel), ChannelId, userId);
        
        return true;
    }

    private async Task SendToAllUsers(string methodName, params object?[] args)
    {
        var tasks = new Task[_users.Count];
        var i = 0;
        var message = new InvocationMessage(methodName, args).AsImmutable<InvocationMessage>();
        foreach (var user in _users)
        {
            var hubUserGrain = _hubContext.User(user.ToString());
            tasks[i++] = hubUserGrain.Send(message);
        }
        await Task.WhenAll(tasks);
    }
}