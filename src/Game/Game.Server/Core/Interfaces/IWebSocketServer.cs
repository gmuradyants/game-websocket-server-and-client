using System.Net.WebSockets;

namespace Game.Server.Core.Interfaces;

public interface IWebSocketServer
{
    event Action<string>? OnWebSocketConnected;
    event Action<string>? OnWebSocketDisconnected;

    /// <summary>
    /// Event that is triggered when a message is received from a WebSocket connection.
    /// </summary>
    event Action<string, string>? OnMessageReceived;

    /// <summary>
    /// Starts a WebSocket session for handling incoming WebSocket connections.
    /// </summary>
    Task StartWebSocketSession(HttpContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Sends a message to a WebSocket
    /// </summary>
    Task SendMessageAsync(string socketSessionId, string message);

    /// <summary>
    /// Sends a close message to a WebSocket
    /// </summary>
    Task CloseMessageAsync(string socketSessionId, WebSocketCloseStatus webSocketCloseStatus, string? description = null);
}