using Akka.Actor;

namespace SysProg.Actors;

public sealed class Shutdown {};

public class App: UntypedActor
{
    public App()
    {
        Context.ActorOf(Props.Create<LoggerActor>(), "Logger");
        Context.ActorOf(Props.Create<ApiServiceActor>(), "ApiService");
        Context.ActorOf(Props.Create<DataManagerActor>(), "DataManager");
        Context.ActorOf(Props.Create<HttpListenerActor>("http://localhost:8080/", 16), "HttpListener");
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