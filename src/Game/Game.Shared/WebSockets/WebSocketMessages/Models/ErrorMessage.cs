namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class ErrorMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.Error;
    public string? Error { get; set; }

    public ErrorMessage(string? error)
    {
        Error = error;
    }
}