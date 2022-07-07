using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using Webclock.Research.SignalR.Messages;
using Webclock.Research.SignalR.Services;

namespace Webclock.Research.SignalR.Hubs
{
    public class AuctionHub : Hub
    {
        private readonly ISignalRProcessor _processor;

        public AuctionHub(ISignalRProcessor processor)
        {
            _processor = processor;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await _processor.ClientConnectedAsync(Context.ConnectionId, GetConnectionType());
        }

        private string GetConnectionType()
        {
            var httpContext = Context.GetHttpContext();
            
            return httpContext != null && httpContext.Request.Query.ContainsKey("type")
                ? httpContext.Request.Query["type"].ToString()
                : "client";
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await _processor.ClientDisconnectedAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public void StartClock()
        {
            _processor.DeliverStartClockRequestByMonitor(Context.ConnectionId, new StartClockRequestByMonitor());
        }

        public void AddBid(AddBid addBid)
        {
            _processor.DeliverAddBid(Context.ConnectionId, addBid);
        }

        public void BuyIntentionQuit(BuyIntentionQuit buyIntentionQuit)
        {
            var serverAuctionHubReceiveTimestamp = Stopwatch.GetTimestamp();

            buyIntentionQuit = new BuyIntentionQuit(
                Context.ConnectionId,
                buyIntentionQuit.ServerConnectionActorSendTimestamp,
                buyIntentionQuit.ServerAuctionHubSendTimestamp,
                buyIntentionQuit.ClientTimeSinceClockStartMs,
                serverAuctionHubReceiveTimestamp);

            _processor.DeliverBuyIntentionQuit(Context.ConnectionId, buyIntentionQuit);
        }
    }
}
