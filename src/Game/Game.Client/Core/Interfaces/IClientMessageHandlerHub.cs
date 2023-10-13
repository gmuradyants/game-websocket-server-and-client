using Game.Shared.WebSockets.Interfaces;

namespace Game.Client.Core.Interfaces;

public interface IClientMessageHandlerHub : IMessageHandlerHub
{
    void GiftReceivedHandler();
    void ErrorHandler();
    void BalanceUpdatedHandler();
    void NotificationHandler();
    void UserLoggedInHandler();
}