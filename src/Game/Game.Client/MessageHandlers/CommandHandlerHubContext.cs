namespace Game.Client.MessageHandlers;

public class CommandHandlerHubContext
{
    public string Message { get; }

    public CommandHandlerHubContext(string message)
    {
        Message = message;
    }
}
