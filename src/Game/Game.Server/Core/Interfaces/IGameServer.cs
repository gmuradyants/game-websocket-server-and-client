namespace Game.Server.Core.Interfaces;

public interface IGameServer
{
    public void Initialize();

    /// <summary>
    /// Handles incoming HTTP requests and establishes WebSocket connections if requested.
    /// </summary>
    Task HandleRequest(HttpContext context);
}