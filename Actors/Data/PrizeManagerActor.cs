namespace SysProg.Actors.Data;

public class PrizeManagerActor: ReceiveActor
{
    Dictionary<int, (int Sum, int Count)> yearlyAverages;

    public PrizeManagerActor()
    {
        yearlyAverages = new ();

        Receive<Prize>(prize =>
        {
            AddAverage(prize.year, prize.prizeAmountAdjusted);

            var name = $"{prize.year}-{prize.id}";
            Context.ActorOf(Props.Create(() => new DataActor<Prize>(prize)), name);
        });

        Receive<YearSpan>(span =>
        {
            var average = SpanAverage(span);
            Sender.Tell(average);
        });
    }

    private float SpanAverage(YearSpan span)
    {
        int sum = 0;
        int count = 0;

        for (int year = span.From; year <= span.To; year++)
        {
            if (yearlyAverages.TryGetValue(year, out var average))
            {
                sum += average.Sum;
                count += average.Count;
            }
        }

        return count > 0 ? sum / count : 0;
    }

    private void AddAverage(int year, int prizeAmountAdjusted)
    {
        if (yearlyAverages.TryGetValue(year, out var average))
        {
            average.Sum += prizeAmountAdjusted;
            average.Count++;
        }
        else
        {
            yearlyAverages[year] = (prizeAmountAdjusted, 1);
        }
    }
}