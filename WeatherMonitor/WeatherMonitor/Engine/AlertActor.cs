using Akka.Actor;
using Akka.Event;
using Microsoft.AspNetCore.SignalR;

namespace WeatherMonitor.Engine;

public class AlertActor : ReceiveActor
{
    public AlertActor(string city, IHubContext<WeatherHub> hub)
    {
        Receive<AlertStarted>(_ =>
        {
            Context.GetLogger().Info("Sending alert notification via SignalR");
            hub.Clients.All.SendAsync("NotifyAlert", city, true);
            Context.Parent.Tell(new AlertNotified(city));
            Self.GracefulStop(TimeSpan.FromSeconds(1));
        });
        
        Receive<AlertStopped>(_ =>
        {
            Context.GetLogger().Info("Sending notification is all good via SignalR");
            hub.Clients.All.SendAsync("NotifyAlert", city, false);
            Context.Parent.Tell(new AlertNotified(city));
            Self.GracefulStop(TimeSpan.FromSeconds(1));
        });
    }
}
