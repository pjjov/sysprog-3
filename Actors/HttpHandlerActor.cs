namespace SysProg.Actors;

public class HttpHandlerActor: ReceiveActor
{
    public record Request(HttpListenerContext ctx);
    public record Response(HttpListenerContext ctx);
    private IActorRef? dataManager;
    public HttpHandlerActor(IActorRef dataManager)
    {
        this.dataManager = dataManager;
        ReceiveAsync<HttpListenerContext>(Handle);
    }

    private async Task Handle(HttpListenerContext ctx)
    {
        var yearSpan = ParseQuery(ctx);
        
        var result = await dataManager.Ask(yearSpan);
        var body =  result!.ToString() ?? "";

        Send(ctx, body);
        Context.Stop(Self);
    }

    private YearSpan ParseQuery(HttpListenerContext ctx)
    {
        var query = ctx.Request.QueryString;

        var fromParam = query["from"];
        var toParam = query["to"];

        if (fromParam == null || toParam == null)
            throw new Exception("Both 'from' and 'to' query parameters must be specified!");
        
        int from, to;

        if (!int.TryParse(fromParam, out from) || !int.TryParse(toParam, out to))
            throw new Exception("Query parameters must be numbers");

        if (from > to)
            throw new Exception("Invalid year span");
        
        return new YearSpan(from, to);
    }

    private void Send(HttpListenerContext context, string body) {
        HttpListenerResponse response = context.Response;

        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(body);
        response.ContentLength64 = buffer.Length;

        System.IO.Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
    }

    public static Props Props(IActorRef dataManager) =>
        Akka.Actor.Props.Create(() => new HttpHandlerActor(dataManager));
}