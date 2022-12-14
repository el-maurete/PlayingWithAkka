using Microsoft.AspNetCore.SignalR.Client;

var url = Environment.GetEnvironmentVariable("HUB_URL") ?? "http://127.0.0.1:5003";

var connection = new HubConnectionBuilder()
    .WithUrl(new Uri(url) + "hub/weather")
    .WithAutomaticReconnect()
    .Build();

connection.Reconnecting += _ => Task.Run(() => Console.WriteLine("Connection lost. Reconnecting..."));
connection.Reconnected += _ => Task.Run(() => Console.WriteLine("Reconnected!"));

connection.On<string, bool>("NotifyAlert", (city, onAlert) =>
    Console.WriteLine(onAlert
        ? $"{city}: DING DING DING DING!!!"
        : $"{city}: Alert is over."));

for (var attempt = 0; attempt < 10; attempt++)
{
    try
    {
        Console.WriteLine("Connecting...");
        await connection.StartAsync();
        Console.WriteLine("Connected!");
        break;
    }
    catch (Exception) { Thread.Sleep(1000); }
}

await connection.SendAsync("StartMonitoring", "London");
await connection.SendAsync("StartMonitoring", "Rome");
await connection.SendAsync("StartMonitoring", "Madrid");

Thread.Sleep(Timeout.Infinite);
