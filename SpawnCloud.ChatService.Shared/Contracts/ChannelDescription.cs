namespace SpawnCloud.ChatService.Shared.Contracts;

public record ChannelDescription
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}