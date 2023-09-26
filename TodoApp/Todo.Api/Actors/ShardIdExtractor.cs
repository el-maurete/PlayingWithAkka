using Akka.Cluster.Sharding;
using Todo.Core;

namespace Todo.Api.Actors;

public class ShardIdExtractor : HashCodeMessageExtractor
{
    public ShardIdExtractor(int maxNumberOfShards) : base(maxNumberOfShards) { }

    public override string EntityId(object message)
    {
        return message is ICommands c
            ? c.Id
            : "not-a-command-" + message.GetType();
    }
}