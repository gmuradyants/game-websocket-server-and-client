using Game.Shared.WebSockets.Interfaces;
using Game.Shared.WebSockets.WebSocketMessages.Models;
using Newtonsoft.Json;

namespace Game.Shared.WebSockets.WebSocketMessages;

public class MessageFactory : IMessageFactory
{
    private readonly Dictionary<CommandType, Func<object[], BaseMessage>> _registry = new();

    public MessageFactory()
    {
        // Register the factories for each message type
        _registry[CommandType.Error] = args => new ErrorMessage(args[0].ToString());
        _registry[CommandType.Login] = args => new LoginMessage(args[0].ToString());

        _registry[CommandType.SendGift] = args => new SendGiftMessage(Convert.ToInt32(args[0]), args[1].ToString(), Convert.ToDouble(args[2].ToString()));
        _registry[CommandType.UpdateResource] = args => new UpdateResourcesMessage(args[0].ToString(), Convert.ToDouble(args[1].ToString()));
        _registry[CommandType.BalanceUpdated] = args => new BalanceUpdatedMessage(Convert.ToDouble(args[0].ToString()), args[1].ToString());

        _registry[CommandType.GiftReceived] = args => new GiftReceivedMessage(Convert.ToInt32(args[0]), args[1].ToString(), Convert.ToDouble(args[2].ToString()));
        _registry[CommandType.Notification] = args => new NotificationMessage(args[0].ToString());
        _registry[CommandType.UserLoggedIn] = args => new UserLoggedInMessage(Convert.ToInt32(args[0]));
    }

    public string CreateMessage(CommandType commandType, params object[] args)
    {
        if (_registry.TryGetValue(commandType, out var value))
        {
            var message = value(args);
            return Serialize(message);
        }

        throw new ArgumentException($"Unsupported message type: {commandType}");
    }

    public TMessage ConvertJsonMessage<TMessage>(string json) where TMessage : BaseMessage
    {
        var jsonObj = JsonConvert.DeserializeObject<dynamic>(json);
        if (jsonObj?.CommandType == null)
            throw new InvalidOperationException("Invalid message format");

        if (!Enum.TryParse(jsonObj.CommandType.ToString(), out CommandType _))
            throw new InvalidOperationException($"Unknown message type '{jsonObj.Type}'");

        var message = JsonConvert.DeserializeObject<TMessage>(json);

        if (message is null)
            throw new InvalidOperationException("Invalid message format");

        return message;
    }

    private static string Serialize(BaseMessage baseMessage)
    {
        return JsonConvert.SerializeObject(baseMessage);
    }
}