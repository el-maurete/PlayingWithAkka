using Akka.Actor;
using Akka.DependencyInjection;

namespace WeatherMonitor;

public interface IActorFactory<T> where T : ActorBase
{
    IActorRef Build(IUntypedActorContext context, string id);
}

public class ActorFactory<T> : IActorFactory<T> where T: ActorBase
{
    public IActorRef Build(IUntypedActorContext context, string id)
    {
        var child = context.Child(id);
        return child is Nobody
            ? context.ActorOf(DependencyResolver.For(context.System).Props<T>(id), id)
            : child;
    }
}
