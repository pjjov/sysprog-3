namespace SysProg.Actors;

public class App: UntypedActor
{
    public sealed record Shutdown;

    public App()
    {
        var loggerActor = Context.ActorOf(Props.Create<LoggerActor>(), "Logger");
        loggerActor.Tell(new LoggerActor.AddConsole());
        loggerActor.Tell(new LoggerActor.AddFile("logs.txt"));

        var apiService = Context.ActorOf(Props.Create<ApiServiceActor>(), "ApiService");
        var dataManager = Context.ActorOf(Props.Create<DataManagerActor>(), "DataManager");
        var httpListener = Context.ActorOf(Props.Create<HttpListenerActor>("http://localhost:8080/", 16), "HttpListener");

        var logger = new Logger(loggerActor);
        dataManager.Tell(new Inject<Logger>(logger));
        apiService.Tell(new Inject<Logger>(logger));
        httpListener.Tell(new Inject<Logger>(logger));

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