namespace SysProg.Actors.Data;

public class LaureateManagerActor: ReceiveActor
{
    public LaureateManagerActor()
    {
        Receive<Laureate>(laureate =>
        {
            var str = $"{laureate.PrizeYear}-{laureate.Id}";
            Context.ActorOf(Props.Create(() => new DataActor<Laureate>(laureate)), str);
        });

        ReceiveAsync<YearSpan>(async yp =>
        {
            var replyTo = Sender;
            // moze bolje vrv ?
            var tasks = Context.GetChildren()
                .Where(child =>
                {
                    var parts = child.Path.Name.Split('-');
                    return parts.Length >= 2
                        && int.TryParse(parts[0], out var year)
                        && year >= yp.From
                        && year <= yp.To;
                })
                .Select(child => child.Ask<Laureate>(
                    null,
                    TimeSpan.FromSeconds(5)))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            replyTo.Tell(results);
        });
    }
}