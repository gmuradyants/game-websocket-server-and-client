namespace Game.DataAccess.Context.Entities;

public sealed class Gift
{
    public int Id { get; set; }
    public int SenderPlayerId { get; set; }
    public Player Sender { get; set; }
    public int ReceiverPlayerId { get; set; }
    public Player Receiver { get; set; }
    public int ResourceTypeId { get; set; }
    public ResourceType ResourceType { get; set; }
    public double ResourceValue { get; set; }
}