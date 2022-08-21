using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Server.Grains;

public class ChatChannelGrain : Grain, IChatChannelGrain
{
    private readonly ILogger<ChatChannelGrain> _logger;
    private readonly HashSet<Guid> _users = new();
    private IAsyncStream<ChatMessage> _stream = null!;
    private readonly Dictionary<Guid, ChatMessage> _messages = new();

    public Guid ChannelId => this.GetPrimaryKey();

    public ChatChannelGrain(ILogger<ChatChannelGrain> logger)
    {
        _logger = logger;
    }

    public override Task OnActivateAsync()
    {
        var streamProvider = GetStreamProvider("chat");

        _stream = streamProvider.GetStream<ChatMessage>(ChannelId, "default");
        
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
            await _stream.OnNextAsync(message);
        }
    }

    public Task<bool> JoinChannel(IChatUserGrain chatUserGrain)
    {
        _users.Add(chatUserGrain.GetPrimaryKey());
        return Task.FromResult(true);
    }
}