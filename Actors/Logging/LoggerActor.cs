namespace SysProg.Actors.Logging;

public class LoggerActor : ReceiveActor
{
    public record AddConsole();
    public record AddFile(string filePath);
    public record Write<T>(T item);
    public record Message(string Category, ActorPath Path, string Content);

    public LoggerActor()
    {
        Receive<AddFile>(cmd => Context.ActorOf(Props.Create<FileLoggerActor>(cmd.filePath)));
        Receive<AddConsole>(cmd => Context.ActorOf(Props.Create<ConsoleLoggerActor>()));
        Receive<Write<HttpHandlerActor.Response>>(write => Handle(write.item));
        Receive<Write<HttpHandlerActor.Request>>(write => Handle(write.item));
        Receive<Write<Exception>>(write => Handle(write.item));
        Receive<Write<string>>(write => Handle(write.item));
        Receive<Write<Prize>>(write => Handle(write.item));
        Receive<Write<Laureate>>(write => Handle(write.item));
    }

    private void Broadcast(string category, string content)
    {
        foreach (IActorRef child in Context.GetChildren())
        {
            child.Tell(new Message(category, Sender.Path, content), Self);
        }
    }

    private void Handle(Prize p)
    {
        Broadcast("PRIZE", $"{p.id}: {p.year} {p.category} {p.prizeAmountAdjusted}");
    }

    private void Handle(Laureate l)
    {
        Broadcast("LAUREATE", $"{l.Id}: {l.Fullname}, {l.Category} {l.PrizeYear}");
    }

    private void Handle(HttpHandlerActor.Response res)
    {
        Broadcast("RESPONSE", $"{res.ctx.Request.HttpMethod} {res.ctx.Request.RawUrl} {res.ctx.Response.StatusCode}");
    }

    private void Handle(HttpHandlerActor.Request req)
    {
        Broadcast("REQUEST", $"{req.ctx.Request.HttpMethod} {req.ctx.Request.RawUrl}");
    }

    private void Handle(Exception ex)
    {
        Broadcast("ERROR", $"{ex.Message} \n{ex.StackTrace}");
    }

    private void Handle(string str)
    {
        Broadcast("MESSAGE", str);
    }
}