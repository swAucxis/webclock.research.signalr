using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Webclock.Research.SignalR.Messages;

var waitHandle = new AutoResetEvent(false);

Task.Run(async () =>
{
    try
    {
        long _clientReceiveStartClockTimestamp = -1;
        StartClock _startClock = null;

        var connection = new HubConnectionBuilder()
            .AddJsonProtocol()
            .WithUrl("https://localhost:7246/auctionHub")
            .Build();

        connection.On("StartClock", (StartClock startClock) =>
        {
            try
            {
                _clientReceiveStartClockTimestamp = Stopwatch.GetTimestamp();
                _startClock = startClock;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in StartClock: {ex.GetBaseException().Message}");
            }
        });

        connection.On("BidReceived", (BidReceived bidReceived) =>
        {
            try
            {
                var clientReceiveBidReceivedTimestamp = Stopwatch.GetTimestamp();
                var clientTimeSinceClockStartMs = (clientReceiveBidReceivedTimestamp - _clientReceiveStartClockTimestamp) / (Stopwatch.Frequency / 1000);

                connection.SendAsync("BuyIntentionQuit", new BuyIntentionQuit(
                    connection.ConnectionId,
                    _startClock.ServerConnectionActorSendTimestamp,
                    _startClock.ServerAuctionHubSendTimestamp,
                    clientTimeSinceClockStartMs)).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in BidReceived: {ex.GetBaseException().Message}");
            }
        });

        await connection.StartAsync();
        Console.WriteLine($"Client connected.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERROR in Program: {ex.GetBaseException().Message}");
    }
});

Console.CancelKeyPress += (o, e) =>
{
    Console.WriteLine("Exit");
    waitHandle.Set();
};

waitHandle.WaitOne();