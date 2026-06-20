using Akka.Actor;

namespace SysProg.Actors;

public sealed class Shutdown {};

public class App: UntypedActor
{
    public App()
    {
    }

    protected override void OnReceive(object message)
    {
        throw new NotImplementedException();
    }

    protected override void PostStop()
    {
        foreach (var child in Context.GetChildren())
            child.Tell(new Shutdown());
        base.PostStop();
    }
}