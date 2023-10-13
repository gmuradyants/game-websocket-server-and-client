namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class UserLoggedInMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.UserLoggedIn;
    public int PlayerId { get; set; }

    public UserLoggedInMessage(int playerId)
    {
        PlayerId = playerId;
    }
}
