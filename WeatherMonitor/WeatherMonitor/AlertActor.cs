using Akka.Actor;
using Akka.Event;
using Microsoft.AspNetCore.SignalR;

namespace WeatherMonitor;

public class AlertActor : ReceiveActor
{
    public AlertActor(string city, IHubContext<WeatherHub> hub)
    {
        Receive<AlertStarted>(_ =>
        {
            Context.GetLogger().Info("Sending alert via SignalR");
            hub.Clients.All.SendAsync("NotifyAlert", city);
            Context.Parent.Tell(new AlertNotified(city));
            Self.GracefulStop(TimeSpan.FromSeconds(1));
        });
    }
}
