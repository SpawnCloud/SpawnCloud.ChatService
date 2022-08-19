namespace SpawnCloud.ChatService.Grains.Models;

public record ChatMessage
{
    public Guid UserId { get; init; }
    public string Message { get; init; } = string.Empty;
}