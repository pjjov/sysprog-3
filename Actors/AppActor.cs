namespace SysProg.Actors;

public class App: UntypedActor
{
    public sealed record Shutdown;

    public App()
    {
        var logger = Context.ActorOf(Props.Create<LoggerActor>(), "Logger");
        logger.Tell(new LoggerActor.AddConsole());
        logger.Tell(new LoggerActor.AddFile("logs.txt"));

        var apiService = Context.ActorOf(Props.Create<ApiServiceActor>(), "ApiService");
        var dataManager = Context.ActorOf(Props.Create<DataManagerActor>(), "DataManager");
        var httpListener = Context.ActorOf(Props.Create<HttpListenerActor>("http://localhost:8080/", 16), "HttpListener");

        dataManager.Tell(new InjectActor<ApiServiceActor>(apiService));
        httpListener.Tell(new InjectActor<DataManagerActor>(dataManager));

        httpListener.Tell(new HttpListenerActor.Start());
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