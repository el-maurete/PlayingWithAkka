using Akka.Actor;
using Akka.Event;

namespace WeatherMonitor;

public class AlertActor : ReceiveActor
{
    public AlertActor(string city)
    {
        Receive<AlertStarted>(_ =>
        {
            var log = Context.GetLogger();
            // Pretend doing something
            Thread.Sleep(1000);
            log.Warning("DING DING DING!");
            Context.Parent.Tell(new AlertNotified(city));
            Self.GracefulStop(TimeSpan.FromSeconds(1));
        });
    }
}
