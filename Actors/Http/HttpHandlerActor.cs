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
        string body = await Query(ctx);
        logger.Write(new Response(ctx));
        
        Send(ctx, body);
        Context.Stop(Self);
    }

    private async Task<string>Query(HttpListenerContext ctx)
    {
        try
        {
            var yearSpan = YearSpan.ParseQuery(ctx);
            var result = await dataManager.Ask(yearSpan);
            return result!.ToString() ?? "";
        }
        catch (Exception e)
        {
            logger.Write(e);
            ctx.Response.StatusCode = 400;
            return e.Message;
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

    public static Props Props(Logger logger, IActorRef dataManager) =>
        Akka.Actor.Props.Create(() => new HttpHandlerActor(logger, dataManager));
}