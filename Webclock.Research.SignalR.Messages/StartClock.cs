namespace Webclock.Research.SignalR.Messages
{
    public class StartClock
    {
        public long ServerConnectionActorSendTimestamp { get; set; }
        public long ServerAuctionHubSendTimestamp { get; set; }

        public StartClock() { }

        public StartClock(
            long serverConnectionActorSendTimestamp)
        {
            ServerConnectionActorSendTimestamp = serverConnectionActorSendTimestamp;
        }

        public StartClock(
            long serverConnectionActorSendTimestamp,
            long serverAuctionHubSendTimestamp)
        {
            ServerConnectionActorSendTimestamp = serverConnectionActorSendTimestamp;
            ServerAuctionHubSendTimestamp = serverAuctionHubSendTimestamp;
        }
    }
}
