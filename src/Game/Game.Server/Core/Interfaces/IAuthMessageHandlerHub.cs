using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Attributes;

namespace Game.Server.Core.Interfaces;

[CommandHandlerHub]
public interface IAuthMessageHandlerHub : IMessageHandlerHub
{
    Task LoginHandler(CancellationToken cancellationToken = default);
}