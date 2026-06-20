using System.Net;
using Akka.Actor;

namespace SysProg.Actors;

public record YearSpan(int From, int To);

public class HttpHandlerActor: ReceiveActor
{
    public HttpHandlerActor()
    {
        Receive<Request>(request => Handle(request));
    }

    private async void Handle(Request request)
    {
        var yearSpan = ParseQuery(request.ctx);
        
        var dataManager = await Context.ActorSelection("../DataManager").ResolveOne(new TimeSpan(0, 5, 0));
        var result = await dataManager.Ask(yearSpan);

        Context.Sender.Tell(new Response(request.ctx, result!.ToString() ?? ""));
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
        
        return new YearSpan(from, to);
    }
}