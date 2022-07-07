namespace Webclock.Research.SignalR.Messages
{
    public class BuyIntentionQuit
    {
        public string ConnectionId { get; set; }
        public long ServerConnectionActorSendTimestamp { get; set; }
        public long ServerAuctionHubSendTimestamp { get; set; }
        public long ClientTimeSinceClockStartMs { get; set; }
        public long ServerAuctionHubReceiveTimestamp { get; set; }

        public BuyIntentionQuit() { }

        public BuyIntentionQuit(
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

        public BuyIntentionQuit(
            string connectionId, 
            long serverConnectionActorSendTimestamp,
            long serverAuctionHubSendTimestamp,
            long clientTimeSinceClockStartMs,
            long serverAuctionHubReceiveTimestamp)
        {
            ConnectionId = connectionId;
            ServerConnectionActorSendTimestamp = serverConnectionActorSendTimestamp;
            ServerAuctionHubSendTimestamp = serverAuctionHubSendTimestamp;
            ClientTimeSinceClockStartMs = clientTimeSinceClockStartMs;
            ServerAuctionHubReceiveTimestamp = serverAuctionHubReceiveTimestamp;
        }
    }
}
