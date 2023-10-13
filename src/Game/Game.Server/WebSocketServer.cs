using System.Collections.Concurrent;
using System.Net.WebSockets;
using Game.Server.Core.Interfaces;
using Game.Shared.WebSockets;
using Serilog;

namespace Game.Server;

/// <summary>
/// Represents a WebSocket server for handling WebSocket connections.
/// </summary>
public class WebSocketServer: IWebSocketServer
{
    public event Action<string>? OnWebSocketConnected;
    public event Action<string>? OnWebSocketDisconnected;
    public event Action<string, string>? OnMessageReceived;

    private readonly ConcurrentDictionary<string, WebSocketSession> _webSocketSession = new();

    public async Task StartWebSocketSession(HttpContext context, CancellationToken cancellationToken)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

        await using var webSocketSession = CreateWebSocketSession(webSocket, cancellationToken , out var success);

        if (success)
        {
            try
            {
                OnWebSocketConnected?.Invoke(webSocketSession.Id);
                webSocketSession.OnMessageReceived += WebSocketSession_OnMessageReceived;

                await webSocketSession.StartAsync();
            }
            finally
            {
                webSocketSession.OnMessageReceived -= WebSocketSession_OnMessageReceived;
                _webSocketSession.TryRemove(webSocketSession.Id, out _);
                OnWebSocketDisconnected?.Invoke(webSocketSession.Id);
            }
        }
    }

    private void WebSocketSession_OnMessageReceived(string webSocketSessionId, 
        string message, WebSocketMessageType webSocketMessageType)
    {
        OnMessageReceived?.Invoke(webSocketSessionId, message);
    }

    private WebSocketSession CreateWebSocketSession(WebSocket webSocket, CancellationToken cancellationToken, out bool success)
    {
        WebSocketSession webSocketSession = default!;

        try
        {
            if (webSocket.State == WebSocketState.Open)
            {
                webSocketSession = new WebSocketSession(webSocket, cancellationToken);
                _webSocketSession.TryAdd(webSocketSession.Id, webSocketSession);
                success = true;
                return webSocketSession;
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to create WebSocket session.");
            success = false;
            return webSocketSession;
        }

        success = true;
        return webSocketSession;
    }

    public async Task SendMessageAsync(string socketSessionId, string message)
    {
        try
        {
            if (_webSocketSession.TryGetValue(socketSessionId, out var webSocketSession))
            {
                await webSocketSession.PushMessageAsync(message);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to sent a message.");
        }
    }

    public async Task CloseMessageAsync(string socketSessionId, WebSocketCloseStatus webSocketCloseStatus, string? description = null)
    {
        try
        {
            if (_webSocketSession.TryGetValue(socketSessionId, out var webSocketSession))
            {
                await webSocketSession.CloseAsync(webSocketCloseStatus, description);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to close a WebSocket.");
        }
    }
}