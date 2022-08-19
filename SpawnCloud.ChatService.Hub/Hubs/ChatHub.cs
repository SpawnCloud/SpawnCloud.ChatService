using Orleans;
using SpawnCloud.ChatService.Contracts.Interfaces;
using SpawnCloud.ChatService.Grains;
namespace SpawnCloud.ChatService.Hub.Hubs;

public class ChatHub : Microsoft.AspNetCore.SignalR.Hub<IChatHubClient>, IChatHub
{
    private readonly IClusterClient _orleansClient;

    public ChatHub(IClusterClient orleansClient)
    {
        _orleansClient = orleansClient;
    }
    
    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        return base.OnDisconnectedAsync(exception);
    }

    public Task SendMessage(Guid userId, string message)
    {
        return Clients.All.ReceiveMessage(userId, message);
    }

    public async Task JoinChannel(Guid channelId)
    {
        var userId = Guid.Parse(Context.UserIdentifier ?? throw new InvalidOperationException());
        var groupId = channelId.ToString("N");

        var chatUserGrain = _orleansClient.GetGrain<IChatUserGrain>(userId);
        //var result = await chatUserGrain.JoinChannel(channelId);

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        await Clients.Group(groupId).UserJoinedChannel(channelId, userId);
    }
}