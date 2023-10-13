namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class GiftReceivedMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.GiftReceived;
    public int FromPlayerId { get; set; }
    public string? ResourceType { get; set; }
    public double ResourceValue { get; set; }

    public GiftReceivedMessage(int fromPlayerId, string? resourceType, double resourceValue)
    {
        FromPlayerId = fromPlayerId;
        ResourceType = resourceType;
        ResourceValue = resourceValue;
    }
}