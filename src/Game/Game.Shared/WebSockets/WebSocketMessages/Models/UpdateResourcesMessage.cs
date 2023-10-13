namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public class UpdateResourcesMessage : BaseMessage
{
    public override CommandType CommandType => CommandType.UpdateResource;
    public string? ResourceType { get; set; }
    public double ResourceValue { get; set; }

    public UpdateResourcesMessage(string? resourceType, double resourceValue)
    {
        ResourceType = resourceType;
        ResourceValue = resourceValue;
    }
}