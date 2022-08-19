using Orleans;
using SpawnCloud.ChatService.Contracts.Models;
using SpawnCloud.ChatService.Grains;

namespace SpawnCloud.ChatService.Server.Grains;

public class ChatChannelGrain : Grain, IChatChannelGrain
{
    public Guid ChannelId => this.GetPrimaryKey();
    
    public Task<ChannelDescription> GetDescription()
    {
        return Task.FromResult(new ChannelDescription
        {
            Id = ChannelId,
            Name = "Test"
        });
    }
}