namespace Game.Shared.WebSockets.WebSocketMessages.Models;

public enum CommandType
{
    Error,
    Login,
    SendGift,
    UpdateResource,
    BalanceUpdated,
    GiftReceived,
    Notification,
    UserLoggedIn
}