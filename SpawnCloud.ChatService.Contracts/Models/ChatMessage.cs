namespace SpawnCloud.ChatService.Contracts.Models;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime SendDate { get; set; } = DateTime.UtcNow;

    public Guid ChannelId { get; set; }
    
    public Guid SenderUserId { get; set; }
    
    public string Body { get; set; } = string.Empty;
}