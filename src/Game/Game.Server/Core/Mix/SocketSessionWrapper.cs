namespace Game.Server.Core.Mix
{
    public class SocketSessionWrapper
    {
        public SocketSessionWrapper(string socketSessionId)
        {
            SocketSessionId = socketSessionId;
        }

        public int PlayerId { get; set; }
        public string SocketSessionId { get; }
        public bool IsAuthenticated { get; set; }
    }
}
