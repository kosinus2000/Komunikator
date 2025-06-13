namespace KomunikatorShared.DTOs;

public class SendMessageRequest
{
    public string ReceiverId {get; set;}
    public string Content { get; set; }
    public DateTime TimeSent { get; set; } = DateTime.UtcNow;
}