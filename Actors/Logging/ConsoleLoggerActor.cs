namespace SysProg.Actors.Logging;

public class ConsoleLoggerActor : ReceiveActor
{
    private bool colored = true;
    
    private record Color(ConsoleColor Foreground, ConsoleColor Background);
    private Dictionary<string, Color> categoryColors;

    public ConsoleLoggerActor()
    {
        categoryColors = new()
        {
            { "REQUEST", new Color(ConsoleColor.White, ConsoleColor.DarkCyan) },
            { "RESPONSE", new Color(ConsoleColor.White, ConsoleColor.Cyan) },
            { "MESSAGE", new Color(ConsoleColor.Black, ConsoleColor.White) },
            { "ERROR", new Color(ConsoleColor.White, ConsoleColor.Red) }
        };

        Receive<LoggerActor.Message>(message =>
        {
            var old = new Color(Console.ForegroundColor, Console.BackgroundColor);
            PrintCategory(message.Category, old);
            PrintPath(message.Path, old);
            PrintColored(message.Content, old);
            Console.Write(Environment.NewLine);
        });
    }

    private void PrintCategory(string category, Color defaultColor)
    {
        var color = categoryColors.GetValueOrDefault(category) ?? defaultColor;
        PrintColored($"{category,-10}", color);
    }

    private void PrintPath(ActorPath path, Color color)
    {
        PrintColored($" {path.Name,-20}", color);
    }

    private void PrintColored(string str, Color color)
    {
        ApplyColor(color);
        Console.Write(str);
    }

    private void ApplyColor(Color color)
    {
        if (colored)
        {
            Console.ForegroundColor = color.Foreground;
            Console.BackgroundColor = color.Background;
        }
    }
}