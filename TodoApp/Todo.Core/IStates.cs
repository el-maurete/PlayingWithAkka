using Akka.Persistence.Fsm;

namespace Todo.Core;

public interface IStates : PersistentFSM.IFsmState
{
    public record State(string Identifier) : IStates;

    public static State NullState = new State("null");

    public static State InProgress = new State(nameof(InProgress));
    public static State Done = new State(nameof(Done));
}