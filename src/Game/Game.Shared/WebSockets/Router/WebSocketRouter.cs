using System.Reflection;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Attributes;
using Game.Shared.WebSockets.Router.Exceptions;
using Game.Shared.WebSockets.WebSocketMessages.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Shared.WebSockets.Router;

/// <summary>
/// Provides routing capabilities for WebSockets based on command types.
/// </summary>
/// <typeparam name="TContext">The type of the context used with the message handler hub.</typeparam>
public class WebSocketRouter<TContext> : IWebSocketRouter<TContext> where TContext : class
{
    private readonly Dictionary<Type, Dictionary<CommandType, HandlerMethodInfo>> _commandsHandlerHubs = new();

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly PropertyInfo _handlerHubContextProperty;

    public WebSocketRouter(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _handlerHubContextProperty = typeof(MessageHandlerHub<TContext>).GetProperty("Context")!;
    }

    public void Initialize()
    {
        ExtractHandlers();
    }

    private void ExtractHandlers()
    {
        var assembly = Assembly.GetEntryAssembly();

        foreach (var type in assembly.GetTypes()
                     .Where(t => t.GetCustomAttributes(typeof(CommandHandlerHubAttribute), true).Length > 0))
        {
            var hubType = type.GetInterfaces()
                .FirstOrDefault(t => typeof(IMessageHandlerHub).IsAssignableFrom(t) && !type.IsAbstract);

            if (hubType == null)
                continue;

            var handlerMethods = type.GetMethods().Where(prop => Attribute.IsDefined(prop, typeof(CommandHandlerAttribute))).ToList();

            var commandHandlerMethod = new Dictionary<CommandType, HandlerMethodInfo>();

            foreach (var handlerMethodInfo in handlerMethods)
            {
                if (handlerMethodInfo.GetCustomAttributes(typeof(CommandHandlerAttribute), false)[0] is CommandHandlerAttribute commandHandlerAttribute)
                {
                    var requiresAuthenticationAttribute = handlerMethodInfo.GetCustomAttributes(typeof(RequiresAuthenticationAttribute), false).FirstOrDefault() as RequiresAuthenticationAttribute;

                    commandHandlerMethod.Add(commandHandlerAttribute.CommandType,
                        new HandlerMethodInfo(handlerMethodInfo, requiresAuthenticationAttribute is not null));
                }
            }

            _commandsHandlerHubs.Add(hubType, commandHandlerMethod);
        }
    }

    public async Task ExecuteHandlerAsync(BaseMessage baseMessage, object context, 
        bool? isClientAuthenticated = null, 
        CancellationToken cancellationToken = default)
    {
        var commandName = baseMessage.CommandType;

        bool commandHandlerFound = default;

        foreach (var commandsHandlerHub in _commandsHandlerHubs)
        {
            if (commandsHandlerHub.Value.TryGetValue(commandName, out var commandHandler) && commandHandler?.MethodInfo is not null)
            {
                commandHandlerFound = true;

                if (commandHandler.RequiresAuthentication)
                {
                    if (isClientAuthenticated == false)
                    {
                        throw new AuthenticationRequiredException();
                    }
                }

                var methodParameters = new List<object>();

                foreach (var parameterType in
                         commandHandler.Parameters.Select(parameter => parameter.ParameterType))
                {
                    if (parameterType == typeof(CancellationToken))
                    {
                        methodParameters.Add(cancellationToken);
                    }
                }

                var methodParam = commandHandler.HasParameters ? methodParameters.ToArray() : null;

                using var scope = _serviceScopeFactory.CreateScope();

                var commandHandlerHubInstance = scope.ServiceProvider.GetService(commandsHandlerHub.Key);

                _handlerHubContextProperty.SetValue(commandHandlerHubInstance, context);

                if (commandHandler.IsAwaitable)
                {
                    await ((Task)commandHandler.MethodInfo.Invoke(commandHandlerHubInstance, methodParam)!);
                }
                else
                {
                    commandHandler.MethodInfo.Invoke(commandHandlerHubInstance, methodParam);
                }

                break;
            }
        }

        if (!commandHandlerFound)
        {
            throw new HandlerNotFoundException();
        }
    }
}