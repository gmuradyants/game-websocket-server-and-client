namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class BaseMessage
{
    public virtual CommandType CommandType { get; set; }
}