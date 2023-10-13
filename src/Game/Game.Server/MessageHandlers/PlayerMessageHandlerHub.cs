using Game.BusinessLogicLayer.Services.Interfaces;
using Game.Server.Core.Interfaces;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Attributes;
using Game.Shared.WebSockets.WebSocketMessages.Models;

namespace Game.Server.MessageHandlers;

public class PlayerCommandHandlerHub : BaseMessageHandlerHub, IPlayerCommandHandlerHub
{
    private readonly IPlayerService _playerService;
    private readonly IGiftService _giftService;

    public PlayerCommandHandlerHub(
        IWebSocketServer webSocketServer,
        IPlayerService playerService,
        IGiftService giftService,
        IMessageFactory messageFactory) : base(webSocketServer, messageFactory)
    {
        _playerService = playerService;
        _giftService = giftService;
    }

    [CommandHandler(CommandType.UpdateResource)]
    [RequiresAuthentication]
    public async Task UpdateResourceHandler(CancellationToken cancellationToken = default)
    {
        var updateResourcesMessage = MessageFactory.ConvertJsonMessage<UpdateResourcesMessage>(Context.Message);

        var resource = await _playerService.AddOrUpdatePlayerResourceAsync(
            Context.SocketSessionWrapper.PlayerId,
            updateResourcesMessage.ResourceType,
            updateResourcesMessage.ResourceValue, cancellationToken);

        var balanceUpdatedMessage = MessageFactory.CreateMessage(CommandType.BalanceUpdated, resource.ResourceValue, updateResourcesMessage.ResourceType!);

        await WebSocketServer.SendMessageAsync(Context.WebSocketSessionId, balanceUpdatedMessage);
    }

    [CommandHandler(CommandType.SendGift)]
    [RequiresAuthentication]
    public async Task SendGiftHandler(CancellationToken cancellationToken = default)
    {
        var sendGiftMessage = MessageFactory.ConvertJsonMessage<SendGiftMessage>(Context.Message);

        var gift = await _giftService.SendGiftAsync(
            sendGiftMessage.FriendPlayerId,
            Context.SocketSessionWrapper.PlayerId,
            sendGiftMessage.ResourceType,
            sendGiftMessage.ResourceValue, cancellationToken);

        if (Context.PlayersMapToSocketSessionWrapper.TryGetValue(sendGiftMessage.FriendPlayerId,
                out var socketSessionWrapper)) //If a friend is online then send a notification about their gift.
        {
            var giftReceivedMessage =
                MessageFactory.CreateMessage(CommandType.GiftReceived, gift.SenderPlayerId, sendGiftMessage.ResourceType!, gift.ResourceValue);
            await WebSocketServer.SendMessageAsync(socketSessionWrapper.SocketSessionId, giftReceivedMessage);
        }
    }
}