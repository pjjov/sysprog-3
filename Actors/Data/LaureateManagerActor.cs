namespace SysProg.Actors.Data;

public class LaureateManagerActor : ReceiveActor
{
    public LaureateManagerActor()
    {
        Receive<Laureate>(laureate =>
        {
            var name = $"{laureate.PrizeYear}-{laureate.Id}";

            if (Context.Child(name).IsNobody())
                Context.ActorOf(Props.Create(() => new DataActor<Laureate>(laureate)), name);
        });

        ReceiveAsync<YearSpan>(async span =>
        {
            var replyTo = Sender;

            var tasks = GetChildren(span)
                .Select(child => child.Ask<Laureate>(
                    new DataActor<Laureate>.Get(),
                    TimeSpan.FromMilliseconds(100)
                ))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            replyTo.Tell(results.ToList());
        });
    }

    private IEnumerable<IActorRef> GetChildren(YearSpan span)
    {
        return Context.GetChildren().Where(child =>
        {
            var parts = child.Path.Name.Split('-');
            return parts.Length >= 2
                && int.TryParse(parts[0], out var year)
                && year >= span.From
                && year <= span.To;
        });
    }
}