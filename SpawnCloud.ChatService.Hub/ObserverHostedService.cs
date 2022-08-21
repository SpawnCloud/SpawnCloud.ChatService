using Orleans;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Hub.Observers;

namespace SpawnCloud.ChatService.Hub;

public interface IChatObserver
{
    Task SubscribeToChannel(Guid channelId);
    Task UnsubscribeFromChannel(Guid channelId);
}

public class ObserverHostedService : IHostedService, IChatObserver
{
    private readonly IClusterClient _orleansClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Guid, Guid> _channels = new();

    public ObserverHostedService(IClusterClient orleansClient, IServiceProvider serviceProvider)
    {
        _orleansClient = orleansClient;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var streamProvider = _orleansClient.GetStreamProvider("chat");
        foreach (var (channelId, handleId) in _channels)
        {
            var stream = streamProvider.GetStream<ChatMessage>(channelId, "default");
            var handles = await stream.GetAllSubscriptionHandles();
            foreach (var handle in handles)
            {
                await handle.UnsubscribeAsync();
            }
        }
    }

    public async Task SubscribeToChannel(Guid channelId)
    {
        if (!_channels.ContainsKey(channelId))
        {
            var streamProvider = _orleansClient.GetStreamProvider("chat");
            var stream = streamProvider.GetStream<ChatMessage>(channelId, "default");
            var handle = await stream.SubscribeAsync(new ChatMessageStreamObserver(channelId, _serviceProvider));
            _channels.Add(channelId, handle.HandleId);
        }
    }

    public async Task UnsubscribeFromChannel(Guid channelId)
    {
        if (_channels.TryGetValue(channelId, out var handleId))
        {
            _channels.Remove(channelId);
            var streamProvider = _orleansClient.GetStreamProvider("chat");
            var stream = streamProvider.GetStream<ChatMessage>(channelId, "default");
            var handles = await stream.GetAllSubscriptionHandles();
            var handle = handles.First(h => h.HandleId == handleId);
            await handle.UnsubscribeAsync();
        }
    }
}