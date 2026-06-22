using System.Security.Cryptography.X509Certificates;

namespace SysProg.Actors.Data;

public class PrizeManagerActor: ReceiveActor
{
    public PrizeManagerActor()
    {
        Receive<Prize>(prize =>
        {
            var str = $"{prize.year}-{prize.id}";
            Context.ActorOf(Props.Create(() => new DataActor<Prize>(prize)), str);
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
                .Select(child => child.Ask<Prize>(
                    null,
                    TimeSpan.FromSeconds(5)))
                .ToArray();

            var results = await Task.WhenAll(tasks);

            replyTo.Tell(results);
        });
    }
}