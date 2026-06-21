using System.Net;
using Akka.Actor;

namespace SysProg.Actors;

public record YearSpan(int From, int To);

public class HttpHandlerActor: ReceiveActor
{
    private IActorRef? dataManager;
    public HttpHandlerActor(HttpListenerContext ctx)
    {
        ReceiveAsync<HttpListenerContext>(Handle);
        Self.Tell(ctx);
    }

    private async Task Handle(HttpListenerContext ctx)
    {
        var yearSpan = ParseQuery(ctx);
        
        var dataManager = await GetDataManager();

        var result = await dataManager.Ask(yearSpan);
        var body =  result!.ToString() ?? "";

        Send(ctx, body);
        Context.Stop(Self);
    }

    private async Task<IActorRef> GetDataManager()
    {
        if (dataManager.IsNobody())
            dataManager = await Context.ActorSelection("../DataManager").ResolveOne(new TimeSpan(0, 5, 0));
        return dataManager!;
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

    public static Props Props(HttpListenerContext ctx) =>
        Akka.Actor.Props.Create(() => new HttpHandlerActor(ctx));
}