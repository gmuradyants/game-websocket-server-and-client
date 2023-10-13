using Game.Shared.WebSockets.WebSocketMessages.Models;

namespace Game.Shared.WebSockets.Interfaces;

public interface IMessageFactory
{
    string CreateMessage(CommandType commandType, params object[] args);
    TMessage ConvertJsonMessage<TMessage>(string json) where TMessage : BaseMessage;
}