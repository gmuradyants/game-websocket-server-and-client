namespace Game.Shared
{
    public static class Constants
    {
        public const int SocketMessageBufferSize = 1024;
        public const int WebSocketServerPort = 5003;
        public static readonly string WebSocketServerUri = $"ws://localhost:{WebSocketServerPort}/ws";

        public static readonly List<string> CommandInstructions = new()
        {
            { "l (Login) <device_id>" },
            { "ur (UpdateResource) <resource_type> <resource_value>" },
            { "sg (SendGift) <friend_player_id> <resource_type> <resource_value>" }
        };
    }
}
