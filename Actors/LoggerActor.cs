
using Akka.Actor;
using SysProg.Actors;
public class LoggerActor : ReceiveActor
{
    public LoggerActor()
    {
        Receive<Response>(msg => HandleRequest(msg));
        Receive<Request>(msg => HandleRequest(msg));        
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

    private void HandleRequest(Response res)
    {
        Broadcast($"[RESPONSE] {res.ctx.Request.QueryString} {res.ctx.Request.HttpMethod} {res.ctx.Response.StatusCode}");
    }

    private void HandleRequest(Request req)
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