using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks.Dataflow;
using Serilog;

namespace Game.Shared.WebSockets;

public class WebSocketSession : IAsyncDisposable
{
    /// <summary>
    /// Raised when a message is received.
    /// </summary>
    /// <remarks>
    /// The event provides the WebSocketSessionId as its first argument 
    /// and the content of the received message as its second argument.
    /// </remarks>
    public event Action<string, string, WebSocketMessageType>? OnMessageReceived;

    private readonly WebSocket _webSocket;
    private readonly BufferBlock<string> _messagesQueue = new();

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly CancellationToken _cancellationToken;
    public string Id { get; }

    public WebSocketSession(WebSocket webSocket, CancellationToken cancellationToken = default)
    {
        _webSocket = webSocket;
        Id = Guid.NewGuid().ToString();
        _cancellationTokenSource = new CancellationTokenSource();
        _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token).Token;
    }

    public Task StartAsync()
    {
        var webSocketSessionTasks = Task.WhenAll(StartReceiveAsync(), StartSendingAsync());
        return webSocketSessionTasks;
    }

    public async Task PushMessageAsync(string message)
    {
        if (_cancellationToken.IsCancellationRequested || _webSocket.State != WebSocketState.Open)
            throw new InvalidOperationException("Push message operation failed, the socket is no longer open");

        try
        {
            var result = await _messagesQueue.SendAsync(message, _cancellationToken);
            if (!result)
            {
                throw new InvalidOperationException("Failed to send the message. The data flow block declined the message.");
            }
        }
        catch (TaskCanceledException)
        {
            //ignore
        }
    }

    public async Task CloseAsync(WebSocketCloseStatus policyViolation = WebSocketCloseStatus.NormalClosure, string? description = null)
    {
        if (_webSocket.State != WebSocketState.Open && _webSocket.State != WebSocketState.CloseReceived)
            throw new InvalidOperationException($"Close operation failed, the socket state {_webSocket.State} is not valid for the closing");

        await _webSocket.CloseAsync(policyViolation, description, _cancellationToken);
    }

    private async Task StartSendingAsync()
    {
        while (!_cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                var message = await _messagesQueue.ReceiveAsync(_cancellationToken);
                var arraySegment = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(arraySegment, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: _cancellationToken);
            }
            catch (WebSocketException ex)
            {
                HandleWebSocketError("WebSocket Error while sending message:", ex);
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }
            catch (Exception e)
            {
                Log.Error($"Error while sending message: {e.Message}");
            }
        }
    }

    private async Task StartReceiveAsync()
    {
        while (!_cancellationToken.IsCancellationRequested && _webSocket.State == WebSocketState.Open)
        {
            try
            {
                var receiveMessage = await ReceiveAsync();
                _ = HandleMessageAsync(receiveMessage.Result, receiveMessage.Message);
            }
            catch (WebSocketException ex)
            {
                HandleWebSocketError("WebSocket Error while receiving message:", ex);
            }
            catch (TaskCanceledException)
            {
                // Ignore
            }
            catch (Exception e)
            {
                Log.Error($"Error while receiving message: {e.Message}");
            }
        }

        _cancellationTokenSource.Cancel();
    }

    private async Task HandleMessageAsync(WebSocketReceiveResult result, byte[] buffer)
    {
        if (result.MessageType == WebSocketMessageType.Text)
        {
            var message = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            OnMessageReceived?.Invoke(Id, message, result.MessageType);
        }
        else if (result.MessageType == WebSocketMessageType.Close)
        {
            var closeStatusDesc = result.CloseStatusDescription
                                  ?? "WebSocket initiated close without description";

            await CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, closeStatusDesc);

            OnMessageReceived?.Invoke(Id, closeStatusDesc!, result.MessageType);
        }
        else if (result.MessageType == WebSocketMessageType.Binary)
        {
            throw new WebSocketException(WebSocketError.InvalidMessageType, "Binary messages are not supported.");
        }
    }

    private async Task<(byte[] Message, WebSocketReceiveResult Result)> ReceiveAsync()
    {
        const int bufferSize = Constants.SocketMessageBufferSize;
        var buffer = new byte[bufferSize];
        var messageBytes = new List<byte>();
        WebSocketReceiveResult result;

        do
        {
            result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken);

            messageBytes.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));

        } while (!result.EndOfMessage);

        return (messageBytes.ToArray(), result);
    }

    private void HandleWebSocketError(string messagePrefix, WebSocketException exception)
    {
        Log.Warning("{@messagePrefix} {@message}", messagePrefix, exception.Message);
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await CloseAsync();
        }
        catch (Exception)
        {
            //ignore
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            _webSocket?.Dispose();
        }
    }
}