using System.Collections.Concurrent;
using System.Net;
using Game.BusinessLogicLayer.Core.Exceptions;
using Game.Server.Core.Interfaces;
using Game.Server.Core.Mix;
using Game.Server.MessageHandlers;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Exceptions;
using Game.Shared.WebSockets.WebSocketMessages.Models;
using Serilog;

namespace Game.Server;

public sealed class GameServer: IGameServer
{
    /// <summary>
    /// WebSocketSessionId, SocketSessionWrapper
    /// </summary>
    private readonly ConcurrentDictionary<string, SocketSessionWrapper> _socketSessionWrapper = new();

    /// <summary>
    /// PlayerId, SocketSessionWrapper
    /// </summary>
    private readonly ConcurrentDictionary<int, SocketSessionWrapper> _playersMapToSocketSessionWrapper = new();

    private readonly IWebSocketServer _webSocketServer;
    private readonly CancellationToken _shutdownToken;
    private readonly IMessageFactory _messageFactory;
    private readonly IWebSocketRouter<CommandHandlerHubContext> _webSocketRouter;

    public GameServer(
        IWebSocketServer webSocketServer,
        IHostApplicationLifetime appLifetime,
        IMessageFactory messageFactory,
        IWebSocketRouter<CommandHandlerHubContext> webSocketRouter)
    {
        _webSocketServer = webSocketServer;
        _webSocketServer.OnWebSocketConnected += WebSocketServer_OnWebSocketConnected;
        _webSocketServer.OnWebSocketDisconnected += WebSocketServer_OnWebSocketDisconnected;
        _webSocketServer.OnMessageReceived += WebSocketServer_OnMessageReceived;
        _messageFactory = messageFactory;
        _shutdownToken = appLifetime.ApplicationStopping;
        _webSocketRouter = webSocketRouter;
    }

    public void Initialize()
    {
        _webSocketRouter.Initialize();
        Log.Information("Game server started.");
    }

    /// <summary>
    /// Handles incoming messages received by the WebSocket server.
    /// </summary>
    /// <param name="socketSessionId">The unique identifier of the WebSocket session.</param>
    /// <param name="message">The received message from the web socket.</param>
    private async void WebSocketServer_OnMessageReceived(string socketSessionId, string message)
    {
        BaseMessage? baseMessage = default;
        try
        {
            baseMessage = _messageFactory.ConvertJsonMessage<BaseMessage>(message);
            Log.Verbose($"A new '{baseMessage.CommandType}' message was received from the client.");

            if (!_socketSessionWrapper.TryGetValue(socketSessionId, out var socketSessionWrapper))
            {
                Log.Error("Client's web socket connection could not be found.");
                return;
            }

            var context =
                new CommandHandlerHubContext(socketSessionId, message, socketSessionWrapper, _playersMapToSocketSessionWrapper);

            await _webSocketRouter.ExecuteHandlerAsync(baseMessage, context, socketSessionWrapper.IsAuthenticated, _shutdownToken);
        }
        catch (HandlerNotFoundException)
        {
            Log.Verbose($"A client requested action with the name '{baseMessage!.CommandType}' does not exists!");

            var notificationMessage = _messageFactory.CreateMessage(CommandType.Notification, $"The action with the name '{baseMessage!.CommandType}' does not exists!");
            await _webSocketServer.SendMessageAsync(socketSessionId, notificationMessage);
        }
        catch (AuthenticationRequiredException)
        {
            Log.Verbose($"A client is not authorized to access to the '{baseMessage!.CommandType}' action!");

            var notificationMessage = _messageFactory.CreateMessage(CommandType.Notification, $"The action '{baseMessage!.CommandType}' requires authentication. Please log in first to proceed.");
            await _webSocketServer.SendMessageAsync(socketSessionId, notificationMessage);
        }
        catch (ValidationException e)
        {
            Log.Information("Sent notification to the client: {@notification}", e.Message);
            var errorMessage = _messageFactory.CreateMessage(CommandType.Error, e.Message);
            await _webSocketServer.SendMessageAsync(socketSessionId, errorMessage);
        }
        catch (Exception e)
        {
            Log.Error("Error while processing the received message: {@error}", e.Message);
        }
    }

    private void WebSocketServer_OnWebSocketConnected(string socketSessionId)
    {
        try
        {
            _socketSessionWrapper.TryAdd(socketSessionId, new SocketSessionWrapper(socketSessionId));
        }
        catch (Exception e)
        {
            Log.Error(e, "WebSocket connected event failed.");
        }
    }

    private void WebSocketServer_OnWebSocketDisconnected(string socketSessionId)
    {
        try
        {
            if (_socketSessionWrapper.TryRemove(socketSessionId, out var socketSessionWrapper))
            {
                _playersMapToSocketSessionWrapper.TryRemove(socketSessionWrapper.PlayerId, out _);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "WebSocket disconnected event failed.");
        }
    }

    public async Task HandleRequest(HttpContext context)
    {
        try
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                Log.Verbose("A new client has been connected to the server");
                await _webSocketServer.StartWebSocketSession(context, _shutdownToken);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Invalid request type. Only WebSocket requests are supported.", cancellationToken: _shutdownToken);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "An unexpected error occurred while processing the WebSocket connection request.");
        }
    }
}