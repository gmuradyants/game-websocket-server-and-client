namespace Game.DataAccess.Context.Entities;

public sealed class ResourceType
{
    public int Id { get; set; }
    public string ResourceTypeName { get; set; }
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}