using Game.Client.Core.Interfaces;
using Game.Client.MessageHandlers;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router;
using Game.Shared.WebSockets.WebSocketMessages;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Client;

public static class ServiceCollectionExtensions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddSingleton<IWebSocketRouter<CommandHandlerHubContext>, WebSocketRouter<CommandHandlerHubContext>>();
        services.AddSingleton<IClientMessageHandlerHub, ClientMessageHandlerHub>();
        services.AddSingleton<IMessageFactory, MessageFactory>();
    }
}