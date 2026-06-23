namespace SysProg.Actors.Logging;

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

        Receive<LoggerActor.Message>(message =>
        {
            var time = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var line = $"{time} {message.Category,-10} {message.Path.ToStringWithoutAddress(),-30} {message.Content}";
            _writer.WriteLine(line);
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