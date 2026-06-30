namespace SysProg.Actors;

public class App : UntypedActor
{
    public sealed record Shutdown;

    public App()
    {
        var loggerActor = Context.ActorOf(Props.Create<LoggerActor>(), "Logger");
        var logger = new Logger(loggerActor);
        loggerActor.Tell(new LoggerActor.AddConsole());
        loggerActor.Tell(new LoggerActor.AddFile("logs.txt"));

        var dataManager = Context.ActorOf(Props.Create<DataManagerActor>(logger), "DataManager");
        var httpListener = Context.ActorOf(Props.Create<HttpListenerActor>(logger, dataManager, "http://localhost:8080/"), "HttpListener");

        var apiService = new ApiService(logger);
        apiService.PrizeStream.Subscribe(prize => dataManager.Tell(prize));
        apiService.LaureateStream.Subscribe(laureate => dataManager.Tell(laureate));

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