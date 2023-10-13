using Game.Shared.WebSockets.WebSocketMessages.Models;

namespace Game.Shared.WebSockets.Router.Attributes;

public class CommandHandlerAttribute : Attribute
{
    public CommandType CommandType { get; }

    public CommandHandlerAttribute(CommandType commandType)
    {
        CommandType = commandType;
    }
}