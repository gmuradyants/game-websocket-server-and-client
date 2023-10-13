using Game.BusinessLogicLayer.Core.Exceptions;
using Game.DataAccess.Context.Entities;
using Serilog;
using System.Net.WebSockets;
using Game.BusinessLogicLayer.Services.Interfaces;
using Game.Server.Core.Interfaces;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Attributes;
using Game.Shared.WebSockets.WebSocketMessages.Models;

namespace Game.Server.MessageHandlers;

public class AuthMessageHandlerHub : BaseMessageHandlerHub, IAuthMessageHandlerHub
{
    private readonly IPlayerService _playerService;

    public AuthMessageHandlerHub(
        IWebSocketServer webSocketServer,
        IPlayerService playerService,
        IMessageFactory messageFactory) : base(webSocketServer, messageFactory)
    {
        _playerService = playerService;
    }

    [CommandHandler(CommandType.Login)]
    public async Task LoginHandler(CancellationToken cancellationToken = default)
    {
        if (Context.SocketSessionWrapper.IsAuthenticated)
        {
            var loginNotificationMessage = MessageFactory.CreateMessage(CommandType.Notification, "You are already logged in.");
            await WebSocketServer.SendMessageAsync(Context.WebSocketSessionId, loginNotificationMessage);
            return;
        }

        var loginMessage = MessageFactory.ConvertJsonMessage<LoginMessage>(Context.Message);

        if (string.IsNullOrWhiteSpace(loginMessage?.DeviceId))
        {
            throw new ValidationException("DeviceId is required.");
        }

        var player = await _playerService.GetPlayerByDeviceIdAsync(loginMessage.DeviceId, cancellationToken)
                     ?? await _playerService.AddPlayerAsync(new Player { DeviceId = loginMessage.DeviceId }, cancellationToken);

        if (Context.PlayersMapToSocketSessionWrapper.TryGetValue(player.Id, out var playerSocketSessionWrapper)) //Player is online
        {
            //The socket for another logged-in user is closed here.
            //But instead of closing, we can inform the user that login was unsuccessful because of an existing connection with the same device ID.

            await WebSocketServer.CloseMessageAsync(
                playerSocketSessionWrapper.SocketSessionId,
                WebSocketCloseStatus.PolicyViolation,
                $"A new client with same DeviceId: {loginMessage.DeviceId} has been connected!");
        }

        Context.SocketSessionWrapper.IsAuthenticated = true;
        Context.SocketSessionWrapper.PlayerId = player.Id;
        Context.PlayersMapToSocketSessionWrapper.TryAdd(player.Id, Context.SocketSessionWrapper);

        Log.Verbose($"A client with deviceID '{loginMessage.DeviceId}' logged in.");

        var notificationMessage = MessageFactory.CreateMessage(CommandType.UserLoggedIn, player.Id);
        await WebSocketServer.SendMessageAsync(Context.WebSocketSessionId, notificationMessage);
    }
}