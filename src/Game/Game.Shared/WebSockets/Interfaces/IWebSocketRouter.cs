using Game.Shared.WebSockets.Router.Exceptions;
using Game.Shared.WebSockets.WebSocketMessages.Models;

namespace Game.Shared.WebSockets.Interfaces;

public interface IWebSocketRouter<TContext>  where TContext : class
{
    /// <summary>
    /// Initializes the router by extracting all the handlers from the current assembly.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Executes the appropriate command handler for the given message.
    /// </summary>
    /// <param name="baseMessage">The base message containing the command to be executed.</param>
    /// <param name="context">The context which the command should use.</param>
    /// <param name="isClientAuthenticated">Optional parameter indicating if the client is authenticated. Default is null.</param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="HandlerNotFoundException">Thrown when a command requires authentication and the client is not authenticated.</exception>
    /// <exception cref="AuthenticationRequiredException">Thrown when no appropriate handler is found for the given command.</exception>
    Task ExecuteHandlerAsync(BaseMessage baseMessage, object context, bool? isClientAuthenticated = null, CancellationToken cancellationToken = default);
}