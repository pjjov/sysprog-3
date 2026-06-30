namespace SysProg.Actors.Http;

public class HttpListenerActor : ReceiveActor
{
    public record Start;
    private sealed record ListenNext;

    private bool shutdown;
    private HttpListener http;
    private Logger logger;
    private IActorRef dataManager;

    public HttpListenerActor(Logger logger, IActorRef dataManager, string prefix)
    {
        if (!HttpListener.IsSupported)
            throw new Exception("HttpListener nije podrzan!");

        http = new();
        http.Prefixes.Add(prefix);
        this.logger = logger;
        this.dataManager = dataManager;

        ReceiveAsync<ListenNext>(_ => Listen());

        ReceiveAsync<Start>(async (_) =>
        {
            http.Start();
            await Listen();
        });

        Receive<App.Shutdown>(_ =>
        {
            shutdown = true;
            http.Stop();
        });
    }

    private async Task Listen()
    {
        try
        {
            var handler = Context.ActorOf(Akka.Actor.Props.Create<HttpHandlerActor>(logger, dataManager));

            var ctx = await http.GetContextAsync();
            handler.Tell(ctx);

            Self.Tell(new ListenNext());
        }
        catch (HttpListenerException e)
        {
            if (shutdown)
                Context.Stop(Self);
            else
                logger.Write(e);
        }
    }

    public static Props Props(Logger logger, IActorRef dataManager, string prefix) =>
        Akka.Actor.Props.Create(() => new HttpListenerActor(logger, dataManager, prefix));
}