using Microsoft.AspNetCore.Authorization;
using OpenIddict.Abstractions;
using Orleans;
using SpawnCloud.ChatService.Contracts.Interfaces;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;
namespace SpawnCloud.ChatService.Hub.Hubs;

[Authorize]
public class ChatHub : Microsoft.AspNetCore.SignalR.Hub<IChatHubClient>, IChatHub
{
    private readonly IClusterClient _orleansClient;
    private readonly IChatObserver _chatObserver;

    public ChatHub(IClusterClient orleansClient, IChatObserver chatObserver)
    {
        _orleansClient = orleansClient;
        _chatObserver = chatObserver;
    }
    
    public override Task OnConnectedAsync()
    {
        return Task.CompletedTask;
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return Task.CompletedTask;
    }

    public async Task SendMessage(ChatMessage message)
    {
        var userId = GetUserId();
        var chatUserGrain = _orleansClient.GetGrain<IChatUserGrain>(userId);
        await chatUserGrain.SendMessage(message);
    }

    public async Task<bool> JoinChannel(Guid channelId)
    {
        var userId = GetUserId();
        var groupId = GetGroupId(channelId);

        var chatUserGrain = _orleansClient.GetGrain<IChatUserGrain>(userId);
        var result = await chatUserGrain.JoinChannel(channelId);
        if (result)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            await Clients.Group(groupId).UserJoinedChannel(channelId, userId);
            await _chatObserver.SubscribeToChannel(channelId);
        }

        return result;
    }

    private Guid GetUserId() => Guid.Parse(Context?.User?.GetClaim("sub") ?? throw new InvalidOperationException());
    
    private static string GetGroupId(Guid channelId) => channelId.ToString("N");
}