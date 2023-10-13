namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class SendGiftMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.SendGift;
    public int FriendPlayerId { get; set; }
    public string? ResourceType { get; set; }
    public double ResourceValue { get; set; }

    public SendGiftMessage(int friendPlayerId, string? resourceType, double resourceValue)
    {
        FriendPlayerId = friendPlayerId;
        ResourceType = resourceType;
        ResourceValue = resourceValue;
    }
}