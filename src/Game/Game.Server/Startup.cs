using Game.BusinessLogicLayer;
using Game.DataAccess.Context;
using Game.Server.Core.Extensions;
using Game.Server.Core.Interfaces;
using Game.Server.Core.Middlewares;
using Game.Server.MessageHandlers;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router;
using Game.Shared.WebSockets.WebSocketMessages;
using Microsoft.EntityFrameworkCore;

namespace Game.Server;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<GameDbContext>(options => options.UseSqlite("Filename=Database/GameDatabase.db"));
        services.AddSingleton<IWebSocketServer, WebSocketServer>();
        services.AddSingleton<IGameServer, GameServer>();
        services.AddSingleton<IMessageFactory, MessageFactory>();
        services.AddScoped<WebSocketServerMiddleware>();
        services.AddScoped<IPlayerCommandHandlerHub, PlayerCommandHandlerHub>();
        services.AddScoped<IAuthMessageHandlerHub, AuthMessageHandlerHub>();
        services.AddSingleton<IWebSocketRouter<CommandHandlerHubContext>, WebSocketRouter<CommandHandlerHubContext>>();

        services.RegisterBllServices();
    }

    public void Configure(IApplicationBuilder app, IGameServer gameServer)
    {
        var webSocketOptions = new WebSocketOptions()
        {
            KeepAliveInterval = TimeSpan.FromMinutes(3),
        };

        app.UseWebSockets(webSocketOptions);

        app.Map("/ws", wsApp =>
        {
            wsApp.UseWebSocketServer();
        });

        gameServer.Initialize();
    }
}