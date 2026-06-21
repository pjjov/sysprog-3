using System.Net;
using Akka.Actor;

namespace SysProg.Actors;

public class HttpListenerActor: ReceiveActor
{
    private sealed class ListenNext {};
    private bool shutdown;
    private HttpListener http;

    public HttpListenerActor(string prefix, int poolSize)
    {
        if (!HttpListener.IsSupported)
            throw new Exception("HttpListener nije podrzan!");

        http = new ();
        http.Prefixes.Add(prefix);

        ReceiveAsync<ListenNext>(Listen);

        Receive<Shutdown>(_ =>
        {
            shutdown = true;
            http.Stop();
        });

        http.Start();
    }

    private async Task Listen(ListenNext _)
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