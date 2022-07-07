namespace Webclock.Research.SignalR.Messages
{
    public class BidReceived
    {
        public long ClientTimeSinceClockStartMs { get; set; }

        public BidReceived() { }

        public BidReceived(
            long clientTimeSinceClockStartMs)
        {
            ClientTimeSinceClockStartMs = clientTimeSinceClockStartMs;
        }
    }
}
