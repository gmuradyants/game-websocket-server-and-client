namespace Game.DataAccess.Context.Entities;

public sealed class Player
{
    public int Id { get; set; }
    public string DeviceId { get; set; }
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public ICollection<Gift> SentGifts { get; set; } = new List<Gift>();
    public ICollection<Gift> ReceivedGifts { get; set; } = new List<Gift>();
}