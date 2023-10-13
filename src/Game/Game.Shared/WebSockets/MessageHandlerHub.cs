using Game.Shared.WebSockets.Router.Attributes;

namespace Game.Shared.WebSockets;

[CommandHandlerHub]
public abstract class MessageHandlerHub<TContext> where TContext : class
{
    public TContext Context { get; set; }
}