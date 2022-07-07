using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Webclock.Research.SignalR.Actors;
using Webclock.Research.SignalR.Hubs;
using Webclock.Research.SignalR.Messages;

namespace Webclock.Research.SignalR.Services
{
    public interface ISignalRProcessor
    {
        Task ClientConnectedAsync(string connectionId, string type);
        Task ClientDisconnectedAsync(string connectionId);

        Task BroadcastStartClock(StartClock startClock);
        Task BroadcastBidReceived(BidReceived bidReceived);

        void DeliverStartClockRequestByMonitor(string connectionId, StartClockRequestByMonitor startClockRequestByMonitor);
        void DeliverAddBid(string connectionId, AddBid addBid);
        void DeliverBuyIntentionQuit(string connectionId, BuyIntentionQuit buyIntentionQuit);

        void AddLatency(string connectionId, Latency latency);
    }

    public class AkkaService : ISignalRProcessor
    {
        private readonly IHubContext<AuctionHub> _hubContext;
        private readonly ActorSystem _actorSystem;

        private List<string> _clients = new List<string>();
        private List<string> _monitors = new List<string>();

        private IDictionary<string, IActorRef> _connectionsActor = new Dictionary<string, IActorRef>();

        private IDictionary<string, Latency> _latency;

        private Timer _latencyReportTimer;

        private static object _lock = new object();

        public AkkaService(IHubContext<AuctionHub> hubContext, ActorSystem actorSystem)
        {
            _hubContext = hubContext;
            _actorSystem = actorSystem;
        }

        public async Task ClientConnectedAsync(string connectionId, string type)
        {
            switch (type)
            {
                case "client":
                    _clients.Add(connectionId);
                    break;
                case "monitor":
                    _monitors.Add(connectionId);
                    break;
            }

            lock (_lock)
            {
                _connectionsActor[connectionId] = GetConnectionActor(connectionId, type);
            }

            await SendConnectedAsync(connectionId);
        }

        private IActorRef GetConnectionActor(string connectionId, string type)
        {
            return _actorSystem.ActorOf(ConnectionActor.Props(connectionId, type, this));
        }

        private async Task SendConnectedAsync(string connectionId)
        {
            await _hubContext.Clients.Client(connectionId).SendAsync("Connected", _clients.Count, _monitors.Count);

            foreach (var monitor in _monitors.Where(x => x != connectionId))
            {
                await _hubContext.Clients.Client(monitor).SendAsync("Connected", _clients.Count, _monitors.Count);
            }
        }

        public async Task ClientDisconnectedAsync(string connectionId)
        {
            _clients.Remove(connectionId);
            _monitors.Remove(connectionId);

            lock (_lock)
            {
                if (_connectionsActor.ContainsKey(connectionId))
                {
                    _actorSystem.Stop(_connectionsActor[connectionId]);
                    _connectionsActor.Remove(connectionId);
                }
            }

            await SendDisconnectedAsync();
        }

        private async Task SendDisconnectedAsync()
        {
            foreach (var monitor in _monitors)
            {
                await _hubContext.Clients.Client(monitor).SendAsync("Disconnected", _clients.Count, _monitors.Count);
            }
        }

        private IActorRef GetConnectionActorByConnectionId(string connectionId)
        {
            lock (_lock)
            {
                return _connectionsActor.GetByKeyOrDefault(connectionId);
            }
        }

        public async Task BroadcastStartClock(StartClock startClock)
        {
            foreach (var client in _clients)
            {
                var startClockPerClient = new StartClock(startClock.ServerConnectionActorSendTimestamp, Stopwatch.GetTimestamp());
                await _hubContext.Clients.Client(client).SendAsync("StartClock", startClockPerClient);
            }
        }

        public async Task BroadcastBidReceived(BidReceived bidReceived)
        {
            _latency = new Dictionary<string, Latency>();
            _latencyReportTimer = new Timer((state) =>
            {
                lock (_lock)
                {
                    foreach (var monitor in _monitors)
                    {
                        _hubContext.Clients.Client(monitor).SendAsync("LatencyReport", _latency.Select(x => x.Value).ToList());
                    }
                }
            }, null, 500, Timeout.Infinite);

            await _hubContext.Clients.Clients(_clients).SendAsync("BidReceived", bidReceived);
        }

        public void AddLatency(string connectionId, Latency latency)
        {
            lock (_lock)
            {
                _latency[connectionId] = latency;
            }
        }

        public void DeliverStartClockRequestByMonitor(string connectionId, StartClockRequestByMonitor startClockRequestByMonitor)
        {
            TellConnectionActorByConnectionId(connectionId, startClockRequestByMonitor);
        }
        public void DeliverAddBid(string connectionId, AddBid addBid)
        {
            TellConnectionActorByConnectionId(connectionId, addBid);
        }

        public void DeliverBuyIntentionQuit(string connectionId, BuyIntentionQuit buyIntentionQuit)
        {
            TellConnectionActorByConnectionId(connectionId, buyIntentionQuit);
        }

        private void TellConnectionActorByConnectionId(string connectionId, object message)
        {
            var connectionActor = GetConnectionActorByConnectionId(connectionId);
            if (connectionActor == null || connectionActor.IsNobody())
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} ERROR: Invalid ConnectionActor! [Context.ConnectionId: {connectionId}]");
                return;
            }

            connectionActor.Tell(message);
        }
    }

    public static class ExtensionMethods
    {
        public static TValue GetByKeyOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            if (dictionary != null && key != null && dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }

            return default;
        }
    }
}
