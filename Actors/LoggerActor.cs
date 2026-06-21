namespace SysProg.Actors;

public class LoggerActor : ReceiveActor
{
    public record AddConsole();
    public record AddFile(string filePath);

    public LoggerActor()
    {
        Receive<AddFile>(filePath => Context.ActorOf(Props.Create<FileLoggerActor>(filePath)));
        Receive<AddConsole>(filePath => Context.ActorOf(Props.Create<ConsoleLoggerActor>(filePath)));
        Receive<HttpHandlerActor.Response>(msg => HandleRequest(msg));
        Receive<HttpHandlerActor.Request>(msg => HandleRequest(msg));
        Receive<Exception>(msg => HandleRequest(msg));
        Receive<string>(msg => HandleRequest(msg));
    }

    private void Broadcast(string message)
    {
        foreach (IActorRef child in Context.GetChildren())
        {
            child.Tell(message, Self);
        }
    }

    private void HandleRequest(HttpHandlerActor.Response res)
    {
        Broadcast($"[RESPONSE] {res.ctx.Request.QueryString} {res.ctx.Request.HttpMethod} {res.ctx.Response.StatusCode}");
    }

    private void HandleRequest(HttpHandlerActor.Request req)
    {
        Broadcast($"[REQUEST] {req.ctx.Request.QueryString} {req.ctx.Request.HttpMethod}");
    }

    private void HandleRequest(Exception ex)
    {
        Broadcast($"[ERROR] {ex.Message}");
    }

    private void HandleRequest(string str)
    {
        Broadcast($"[MESSAGE] {str}");
    }
}