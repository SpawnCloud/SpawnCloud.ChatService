using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Orleans;
using SpawnCloud.ChatService.Contracts.Interfaces;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Hubs;

[Authorize]
public class ChatHub : Hub<IChatHubClient>, IChatHub
{
    private readonly IGrainFactory _grainFactory;

    public ChatHub(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
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
        var chatUserGrain = _grainFactory.GetGrain<IChatUserGrain>(userId);
        await chatUserGrain.SendMessage(message);
    }

    public async Task<bool> JoinChannel(Guid channelId)
    {
        var userId = GetUserId();
        var chatUserGrain = _grainFactory.GetGrain<IChatUserGrain>(userId);
        var result = await chatUserGrain.JoinChannel(channelId);
        return result;
    }

    public async Task LeaveChannel(Guid channelId)
    {
        var userId = GetUserId();
        var chatUserGrain = _grainFactory.GetGrain<IChatUserGrain>(userId);
        await chatUserGrain.LeaveChannel(channelId);
    }

    private Guid GetUserId() => Guid.Parse(Context.UserIdentifier ?? throw new InvalidOperationException());
}