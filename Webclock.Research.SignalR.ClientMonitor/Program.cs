using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Webclock.Research.SignalR.Messages;

var connection = new HubConnectionBuilder()
    .AddJsonProtocol()
    .WithUrl("https://localhost:7246/auctionHub?type=monitor")
    .Build();

connection.On("Connected", (int clientCount, int monitorCount) =>
{
    Console.WriteLine($"Connected [clientCount: {clientCount}][monitorCount: {monitorCount}]");
});

connection.On("LatencyReport", (List<Latency> latency) =>
{
    if (latency.Any())
    {
        Console.WriteLine("***** Latency Report *****");
        var fullLatencyList = latency.Select(x => x.FullLatency).ToList();
        Console.WriteLine($"FullLatency [min: {fullLatencyList.Min()}][avg: {fullLatencyList.Average()}][max: {fullLatencyList.Max()}]");

        var serverSendLatencyList = latency.Select(x => x.ServerSendLatency).ToList();
        Console.WriteLine($"ServerSendLatency [min: {serverSendLatencyList.Min()}][avg: {serverSendLatencyList.Average()}][max: {serverSendLatencyList.Max()}]");

        var serverReceiveLatencyList = latency.Select(x => x.ServerReceiveLatency).ToList();
        Console.WriteLine($"ServerReceiveLatency [min: {serverReceiveLatencyList.Min()}][avg: {serverReceiveLatencyList.Average()}][max: {serverReceiveLatencyList.Max()}]");
        Console.WriteLine("**************************");
        Console.WriteLine("");
        Task.Delay(1000).Wait();

        StartClock().Wait();
    }
});

connection.On("Disconnected", (int clientCount, int monitorCount) =>
{
    Console.WriteLine($"Disconnected [clientCount: {clientCount}][monitorCount: {monitorCount}]");
});

await connection.StartAsync();


Console.WriteLine("Press ENTER to start");
Console.ReadLine();
Console.WriteLine("Process started ...");

await StartClock();

Console.ReadLine();

async Task StartClock()
{
    await connection.SendAsync("StartClock");

    await Task.Delay(5000);

    await connection.SendAsync("AddBid", new AddBid());
}