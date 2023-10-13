using Game.Server.Core.Interfaces;

namespace Game.Server.Core.Middlewares;

public class WebSocketServerMiddleware : IMiddleware
{
    private readonly IGameServer _gameServer;

    public WebSocketServerMiddleware(IGameServer gameServer)
    {
        _gameServer = gameServer;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await _gameServer.HandleRequest(context);
    }
}