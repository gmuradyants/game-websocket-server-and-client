using Game.Server.Core.Interfaces;
using Game.Shared.WebSockets;
using Game.Shared.WebSockets.Interfaces;

namespace Game.Server.MessageHandlers;

public abstract class BaseMessageHandlerHub : MessageHandlerHub<CommandHandlerHubContext>
{
    protected IWebSocketServer WebSocketServer { get; }

    protected IMessageFactory MessageFactory { get; }

    protected BaseMessageHandlerHub(IWebSocketServer webSocketServer, IMessageFactory messageFactory)
    {
        WebSocketServer = webSocketServer;
        MessageFactory = messageFactory;
    }
}