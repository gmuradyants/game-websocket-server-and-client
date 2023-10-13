namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class BalanceUpdatedMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.BalanceUpdated;
    public double NewBalance { get; set; }
    public string? ResourceType { get; set; }

    public BalanceUpdatedMessage(double newBalance, string? resourceType)
    {
        NewBalance = newBalance;
        ResourceType = resourceType;
    }
}