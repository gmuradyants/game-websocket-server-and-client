using Game.Client.Core.Interfaces;
using Game.Shared.WebSockets;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Attributes;
using Game.Shared.WebSockets.WebSocketMessages.Models;
using Serilog;

namespace Game.Client.MessageHandlers;

public class ClientMessageHandlerHub : MessageHandlerHub<CommandHandlerHubContext>, IClientMessageHandlerHub
{
    private readonly IMessageFactory _messageFactory;

    public ClientMessageHandlerHub(IMessageFactory messageFactory)
    {
        _messageFactory = messageFactory;
    }

    [CommandHandler(CommandType.GiftReceived)]
    public void GiftReceivedHandler()
    {
        var giftMessage = _messageFactory.ConvertJsonMessage<GiftReceivedMessage>(Context.Message);

        Log.Information($"Gift received! Gift details: ResourceType - {giftMessage.ResourceType}, Sender - {giftMessage.FromPlayerId}, " +
                        $"ResourceValue - {giftMessage.ResourceValue}");
    }

    [CommandHandler(CommandType.Error)]
    public void ErrorHandler()
    {
        var errorMessage = _messageFactory.ConvertJsonMessage<ErrorMessage>(Context.Message);
        Log.Warning($"Error message received: '{errorMessage.Error}'");
    }

    [CommandHandler(CommandType.BalanceUpdated)]
    public void BalanceUpdatedHandler()
    {
        var balanceMessage = _messageFactory.ConvertJsonMessage<BalanceUpdatedMessage>(Context.Message);
        Log.Information($"Your '{balanceMessage.ResourceType}' have been updated. New balance is: {balanceMessage.NewBalance}");
    }

    [CommandHandler(CommandType.Notification)]
    public void NotificationHandler()
    {
        var message = _messageFactory.ConvertJsonMessage<NotificationMessage>(Context.Message);
        Log.Information($"Notification from server: {message?.Message}");
    }

    [CommandHandler(CommandType.UserLoggedIn)]
    public void UserLoggedInHandler()
    {
        var message = _messageFactory.ConvertJsonMessage<UserLoggedInMessage>(Context.Message);
        Log.Information($"You've successfully logged in. Your unique player ID is {message.PlayerId}.");
    }
}