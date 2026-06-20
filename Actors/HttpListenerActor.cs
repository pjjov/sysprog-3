using System.Net;
using Akka.Actor;

namespace SysProg.Actors;
public record Request(HttpListenerContext ctx);
public record Response(HttpListenerContext ctx, string body);

public class HttpListenerActor: ReceiveActor
{
    private sealed class ListenNext {};
    private int activeRequests;
    private bool shutdown;
    private HttpListener http;
    private IActorRef handler;

    public HttpListenerActor(string prefix, int poolSize)
    {
        if (!HttpListener.IsSupported)
            throw new Exception("HttpListener nije podrzan!");

        handler = Context.ActorOf(Akka.Actor.Props.Create<HttpHandlerActor>(), "handler");

        http = new ();
        http.Prefixes.Add(prefix);

        ReceiveAsync<ListenNext>(Listen);

        Receive<Response>(response =>
        {
            Send(response.ctx, response.body);
            activeRequests--;

            if (shutdown && activeRequests == 0)
                Context.Stop(Self);
        });

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
            handler.Tell(new Request(ctx));
            
            activeRequests++;
            Self.Tell(new ListenNext());
        }
        catch (HttpListenerException) when (shutdown)
        {
            Context.Stop(Self);
        }    
    }

    private void Send(HttpListenerContext context, string body) {
        HttpListenerResponse response = context.Response;

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = buffer.Length;

        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    public static Props Props(string prefix, int poolSize) =>
        Akka.Actor.Props.Create(() => new HttpListenerActor(prefix, poolSize));
}