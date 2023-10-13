namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class LoginMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.Login;
    public string? DeviceId { get; set; }

    public LoginMessage(string? deviceId)
    {
        DeviceId = deviceId;
    }
}