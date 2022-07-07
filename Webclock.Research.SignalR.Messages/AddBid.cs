namespace Webclock.Research.SignalR.Messages
{
    public class AddBid
    {
        public string ConnectionId { get; set; }
        public long ServerConnectionActorSendTimestamp { get; set; }
        public long ServerAuctionHubSendTimestamp { get; set; }
        public long ClientTimeSinceClockStartMs { get; set; }

        public AddBid() { }

        public AddBid(
            string connectionId,
            long serverConnectionActorSendTimestamp,
            long serverAuctionHubSendTimestamp,
            long clientTimeSinceClockStartMs)
        {
            ConnectionId = connectionId;
            ServerConnectionActorSendTimestamp = serverConnectionActorSendTimestamp;
            ServerAuctionHubSendTimestamp = serverAuctionHubSendTimestamp;
            ClientTimeSinceClockStartMs = clientTimeSinceClockStartMs;
        }
    }
}
