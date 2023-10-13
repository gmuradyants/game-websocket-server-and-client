using System.Collections.Concurrent;
using Game.Server.Core.Mix;

namespace Game.Server.MessageHandlers;

public class CommandHandlerHubContext
{
    public string WebSocketSessionId { get; }
    public string Message { get; }
    public SocketSessionWrapper SocketSessionWrapper { get; }
    public ConcurrentDictionary<int, SocketSessionWrapper> PlayersMapToSocketSessionWrapper { get; }

    public CommandHandlerHubContext(string webSocketSessionId, string message,
        SocketSessionWrapper socketSessionWrapper,
        ConcurrentDictionary<int, SocketSessionWrapper> playersMapToSocketSessionWrapper)
    {
        WebSocketSessionId = webSocketSessionId;
        Message = message;
        SocketSessionWrapper = socketSessionWrapper;
        PlayersMapToSocketSessionWrapper = playersMapToSocketSessionWrapper;
    }
}