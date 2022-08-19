namespace SpawnCloud.ChatService.Contracts.Models;

public record ChannelDescription
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}