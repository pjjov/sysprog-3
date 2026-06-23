using System.Text.Json;

namespace SysProg.Actors.Http;

public class HttpHandlerActor: ReceiveActor
{
    public record Request(HttpListenerContext ctx);
    public record Response(HttpListenerContext ctx);
    private IActorRef dataManager = ActorRefs.Nobody;
    private Logger logger = Logger.Nobody;
    public HttpHandlerActor(Logger logger, IActorRef dataManager)
    {
        this.dataManager = dataManager;
        this.logger = logger;
        ReceiveAsync<HttpListenerContext>(Handle);
    }

    private async Task Handle(HttpListenerContext ctx)
    {
        logger.Write(new Request(ctx));
    
        if (ParseRequest(ctx, out var yearSpan))
            await Query(ctx, yearSpan);

        logger.Write(new Response(ctx));
        
        Context.Stop(Self);
    }

    private async Task Query(HttpListenerContext ctx, YearSpan yearSpan)
    {
        var res = ctx.Response;

        try
        {
            var result = await dataManager.Ask<DataManagerActor.Result>(yearSpan);
            var body = JsonSerializer.Serialize(result);

            res.AddHeader("Content-Type", "application/json");
            Send(ctx, body);
        }
        catch (Exception e)
        {
            logger.Write(e);
            Send(ctx, e.Message, 500);
        }
    }

    private bool ParseRequest(HttpListenerContext ctx, out YearSpan span)
    {
        var req = ctx.Request;

        try
        {
            if (req.Url == null || req.Url.LocalPath != "/")
                throw new Exception($"Only '/' path is allowed; got {req.Url?.LocalPath}!");

            span = YearSpan.ParseQuery(req.QueryString);
            return true;
        }
        catch (Exception e)
        {
            Send(ctx, e.Message, 400);
            span = new YearSpan(0, 0);
            return false;
        }
    }

    private void Send(HttpListenerContext context, string body, int code = 200) {
        HttpListenerResponse response = context.Response;
        response.StatusCode = code;

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = buffer.Length;

        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    public static Props Props(Logger logger, IActorRef dataManager) =>
        Akka.Actor.Props.Create(() => new HttpHandlerActor(logger, dataManager));
}