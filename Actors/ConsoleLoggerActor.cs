using Akka.Actor;

public class ConsoleLoggerActor : ReceiveActor
{
    public ConsoleLoggerActor()
    {
        Receive<string>(message =>
        {
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        });
    }
}