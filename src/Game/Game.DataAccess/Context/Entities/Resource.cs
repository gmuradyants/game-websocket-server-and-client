namespace Game.DataAccess.Context.Entities;

public sealed class Resource
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public Player Player { get; set; }
    public int ResourceTypeId { get; set; }
    public ResourceType ResourceType { get; set; }
    public double ResourceValue { get; set; }
}