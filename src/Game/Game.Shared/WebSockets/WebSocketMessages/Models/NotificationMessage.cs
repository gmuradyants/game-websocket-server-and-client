namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class NotificationMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.Notification;
    public string? Message { get; set; }

    public NotificationMessage(string? message)
    {
        Message = message;
    }
}