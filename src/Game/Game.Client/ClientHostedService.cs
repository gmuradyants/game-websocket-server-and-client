using Microsoft.Extensions.Hosting;
using Serilog;
using System.Net.WebSockets;
using Game.Client.MessageHandlers;
using Game.Shared;
using Game.Shared.WebSockets;
using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.Router.Exceptions;
using Game.Shared.WebSockets.WebSocketMessages.Models;
using Polly;

namespace Game.Client;

public sealed class ClientHostedService : IHostedService, IAsyncDisposable
{
    private readonly CancellationTokenSource _cancelTokenSource = new();
    private Task? _executingTask;
    private readonly IHostApplicationLifetime _appLifetime;

    private WebSocketSession? _webSocketSession;
    private readonly IMessageFactory _messageFactory;
    private readonly IWebSocketRouter<CommandHandlerHubContext> _webSocketRouter;

    public ClientHostedService(
        IHostApplicationLifetime appLifetime,
        IMessageFactory messageFactory,
        IWebSocketRouter<CommandHandlerHubContext> webSocketRouter)
    {
        _appLifetime = appLifetime;
        _messageFactory = messageFactory;
        _webSocketRouter = webSocketRouter;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executingTask = StartWebSocketClient(cancellationToken).ContinueWith(ExceptionHandler, TaskContinuationOptions.OnlyOnFaulted);

        if (_executingTask.IsCompleted)
        {
            return _executingTask;
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask == null)
        {
            return;
        }

        try
        {
            _cancelTokenSource.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    public async Task StartWebSocketClient(CancellationToken token)
    {
        _webSocketRouter.Initialize();

        Console.WriteLine("Available Commands:\n");
        foreach (var commandInstructions in Constants.CommandInstructions)
        {
            Console.WriteLine($"- {commandInstructions}");
        }

        Console.WriteLine("\nPress enter to start the client.");
        Console.ReadLine();

        Log.Information("Connecting to the server...");

        using var ws = await ConnectWithRetryAsync(_cancelTokenSource.Token);

        Log.Information("Connection to the server was successful! \n\n You can now use the above commands to interact with the server");

        _webSocketSession = new WebSocketSession(ws, _cancelTokenSource.Token);
        _webSocketSession.OnMessageReceived += WebSocketSession_OnMessageReceived;

        _ = Task.Run(ProcessCommands, _cancelTokenSource.Token);

        await _webSocketSession.StartAsync();
    }

    private async Task<ClientWebSocket> ConnectWithRetryAsync(CancellationToken token)
    {
        ClientWebSocket? ws = null;

        var retryPolicy = Policy
            .Handle<WebSocketException>()
            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(2),
                (exception, timeSpan, retryCount, _) =>
                {
                    Log.Warning($"Attempt {retryCount}: Error connecting to the server, error: '{exception.Message}'. Retrying in {timeSpan.Seconds} seconds...");
                });

        await retryPolicy.ExecuteAsync(async () =>
        {
            ws?.Dispose();
            ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri(Constants.WebSocketServerUri), token);
        });

        return ws!;
    }

    private async Task ProcessCommands()
    {
        while (!_cancelTokenSource.IsCancellationRequested)
        {
            try
            {
                var messageText = Console.ReadLine();

                if (ParseMessage(messageText, out var parsedMessage))
                {
                    await _webSocketSession!.PushMessageAsync(parsedMessage!);
                }
                else
                {
                    Log.Information($"The message '{messageText}' could not be parsed. Please use the correct structure.");
                }
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }
        }
    }

    private bool ParseMessage(string? messageText, out string? parsedMessage)
    {
        parsedMessage = null;

        try
        {
            var messageParts = messageText!.Split(" ");

            if (messageParts[0] == "l")
            {
                var deviceId = messageParts[1];
                parsedMessage = _messageFactory.CreateMessage(CommandType.Login, deviceId);
            }

            if (messageParts[0] == "ur")
            {
                var resourceType = messageParts[1];
                var resourceValue = Convert.ToDouble(messageParts[2]);
                parsedMessage = _messageFactory.CreateMessage(CommandType.UpdateResource, resourceType, resourceValue);
            }

            if (messageParts[0] == "sg")
            {
                var friendPlayerId = Convert.ToInt32(messageParts[1]);
                var resourceType = messageParts[2];
                var resourceValue = Convert.ToDouble(messageParts[3]);
                parsedMessage = _messageFactory.CreateMessage(CommandType.SendGift, friendPlayerId, resourceType, resourceValue);
            }
        }
        catch
        {
            return false;
        }

        return parsedMessage is not null;
    }

    private async void WebSocketSession_OnMessageReceived(string webSocketSessionId, 
        string message, WebSocketMessageType webSocketMessageType)
    {
        BaseMessage baseMessage = default!;
        try
        {
            if (webSocketMessageType == WebSocketMessageType.Close)
            {
                Log.Warning($"Your connection has been terminated for the following reason: {message}.");
                return;
            }

            baseMessage = _messageFactory.ConvertJsonMessage<BaseMessage>(message);

            var context = new CommandHandlerHubContext(message);
            await _webSocketRouter.ExecuteHandlerAsync(baseMessage, context, cancellationToken: _cancelTokenSource.Token);
        }
        catch (HandlerNotFoundException)
        {
            Log.Warning($"A received message handler '{baseMessage!.CommandType}' has not been found.");
        }
        catch (Exception e)
        {
            Log.Error("Error while processing the received message: {@error}", e.Message);
        }
    }

    private void ExceptionHandler(Task task)
    {
        Log.Error(task.Exception, "");
        _appLifetime.StopApplication();
    }

    public async ValueTask DisposeAsync()
    {
        if (_webSocketSession is not null)
        {
            await _webSocketSession.DisposeAsync();
        }

        _cancelTokenSource?.Cancel();
        _cancelTokenSource?.Dispose();
    }
}