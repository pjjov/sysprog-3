namespace SysProg.Actors;

public class HttpListenerActor: ReceiveActor
{
    public record Start;
    private sealed record ListenNext;
    private bool shutdown;
    private HttpListener http;
    private IActorRef dataManager = ActorRefs.Nobody;

    public HttpListenerActor(string prefix, int poolSize)
    {
        if (!HttpListener.IsSupported)
            throw new Exception("HttpListener nije podrzan!");

        http = new ();
        http.Prefixes.Add(prefix);

        Receive<InjectActor<DataManagerActor>>(dep => dataManager = dep.Reference);
        ReceiveAsync<ListenNext>(_ => Listen());
    
        ReceiveAsync<Start>(async (_) => {
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
            var ctx = await http.GetContextAsync();
            Context.ActorOf(Akka.Actor.Props.Create<HttpHandlerActor>(ctx));
            Self.Tell(new ListenNext());
        }
        catch (HttpListenerException) when (shutdown)
        {
            Context.Stop(Self);
        }    
    }

    public static Props Props(string prefix, int poolSize) =>
        Akka.Actor.Props.Create(() => new HttpListenerActor(prefix, poolSize));
}