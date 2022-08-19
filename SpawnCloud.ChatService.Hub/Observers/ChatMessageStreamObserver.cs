using Microsoft.AspNetCore.SignalR;
using Orleans.Streams;
using SpawnCloud.ChatService.Contracts.Interfaces;
using SpawnCloud.ChatService.Grains.Models;
using SpawnCloud.ChatService.Hub.Hubs;

namespace SpawnCloud.ChatService.Hub.Observers;

public class ChatMessageStreamObserver : IAsyncObserver<ChatMessage>
{
    private readonly Guid _channelId;
    private readonly IServiceProvider _serviceProvider;

    public ChatMessageStreamObserver(Guid channelId, IServiceProvider serviceProvider)
    {
        _channelId = channelId;
        _serviceProvider = serviceProvider;
    }
    
    public async Task OnNextAsync(ChatMessage item, StreamSequenceToken? token = null)
    {
        var scope = _serviceProvider.CreateAsyncScope();
        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChatHub, IChatHubClient>>();

        await hubContext.Clients.Group(_channelId.ToString("N")).ReceiveMessage(item.UserId, item.Message);
        
        await scope.DisposeAsync();
    }

    public Task OnCompletedAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnErrorAsync(Exception ex)
    {
        return Task.CompletedTask;
    }
}