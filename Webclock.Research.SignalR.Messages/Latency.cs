
namespace Webclock.Research.SignalR.Messages
{
    public class Latency
    {
        public string ConnectionId { get; set; }
        public long ServerTimeSinceClockStartMs { get; set; }
        public long ClientTimeSinceClockStartMs { get; set; }
        public long FullLatency { get; set; }
        public long ServerSendLatency { get; set; }
        public long ServerReceiveLatency { get; set; }

        public Latency() { }

        public Latency(
            string connectionId,
            long serverTimeSinceClockStartMs,
            long clientTimeSinceClockStartMs,
            long fullLatency,
            long serverSendLatency,
            long serverReceiveLatency)
        {
            ConnectionId = connectionId;
            ServerTimeSinceClockStartMs = serverTimeSinceClockStartMs;
            ClientTimeSinceClockStartMs = clientTimeSinceClockStartMs;
            FullLatency = fullLatency;
            ServerSendLatency = serverSendLatency;
            ServerReceiveLatency = serverReceiveLatency;
        }
    }
}
