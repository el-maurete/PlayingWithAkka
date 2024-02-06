using Akka.Cluster.Sharding;
using Todo.Core;

namespace Todo.Api.Actors;

public class ShardIdExtractor : HashCodeMessageExtractor
{
    public ShardIdExtractor(int maxNumberOfShards) : base(maxNumberOfShards) { }

    public override string EntityId(object message)
    {
        return message switch {
            ShardingEnvelope se => se.EntityId,
            ICommands c => c.Id,
            _ => "not-a-command-" + message.GetType()
        };
    }

    public override object EntityMessage(object message)
    {
        return message switch {
            ShardingEnvelope se => se.Message,
            {} anything => anything
        };
    }
}
