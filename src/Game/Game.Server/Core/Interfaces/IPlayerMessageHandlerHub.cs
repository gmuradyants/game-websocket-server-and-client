using Game.Shared.WebSockets.Interfaces;

namespace Game.Server.Core.Interfaces;

public interface IPlayerCommandHandlerHub : IMessageHandlerHub
{
    Task UpdateResourceHandler(CancellationToken cancellationToken = default);
    Task SendGiftHandler(CancellationToken cancellationToken = default);
}