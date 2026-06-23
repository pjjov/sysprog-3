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
        Receive<Write<HttpHandlerActor.Response>>(write => HandleRequest(write.item));
        Receive<Write<HttpHandlerActor.Request>>(write => HandleRequest(write.item));
        Receive<Write<Exception>>(write => HandleRequest(write.item));
        Receive<Write<string>>(write => HandleRequest(write.item));
    }

    private void Broadcast(string category, string content)
    {
        foreach (IActorRef child in Context.GetChildren())
        {
            child.Tell(new Message(category, Sender.Path, content), Self);
        }
    }

    private void HandleRequest(HttpHandlerActor.Response res)
    {
        Broadcast("RESPONSE", $"{res.ctx.Request.HttpMethod} {res.ctx.Request.RawUrl} {res.ctx.Response.StatusCode}");
    }

    private void HandleRequest(HttpHandlerActor.Request req)
    {   
        Broadcast("REQUEST", $"{req.ctx.Request.HttpMethod} {req.ctx.Request.RawUrl}");
    }

    private void HandleRequest(Exception ex)
    {
        Broadcast("ERROR", $"{ex.Message} \n{ex.StackTrace}");
    }

    private void HandleRequest(string str)
    {
        Broadcast("MESSAGE", str);
    }
}