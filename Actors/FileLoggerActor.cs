using System.Globalization;
using Akka.Actor;

public class FileLoggerActor : ReceiveActor
{
    private readonly StreamWriter _writer;
    public FileLoggerActor(string filePath)
    {
        _writer = new StreamWriter(
            new FileStream(
                filePath,
                FileMode.Append))
        {
            AutoFlush = false
        };

        Receive<string>(message =>
        {
            _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        });
    }

    protected override void PostStop()
    {
        _writer?.Close();
        base.PostStop();
    }

    public static Props Props(string filePath) =>
        Akka.Actor.Props.Create(() => new FileLoggerActor(filePath));
}