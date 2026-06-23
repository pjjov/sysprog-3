namespace SysProg.Actors;

public class DataManagerActor: ReceiveActor
{
    public record Result(float AveragePriceAmountAdjusted, List<Laureate> Laureates);

    private IActorRef prizeManager;
    private IActorRef laureateManager;
    private IActorRef apiService = ActorRefs.Nobody;
    private Logger logger = Logger.Nobody;
    private List<YearSpan> loadedSpans;

    public DataManagerActor()
    {
        loadedSpans = new ();
        prizeManager = Context.ActorOf(Akka.Actor.Props.Create<PrizeManagerActor>(), "PrizeManager");
        laureateManager = Context.ActorOf(Akka.Actor.Props.Create<LaureateManagerActor>(), "LaureateManager");

        ReceiveAsync<YearSpan>(span => FindYearSpan(span));
        
        Receive<Inject<Logger>>(dep => logger = dep.Item);
        Receive<InjectActor<ApiServiceActor>>(dep => {
            apiService = dep.Reference;
            apiService.Tell(new InjectActor<PrizeManagerActor>(prizeManager));
            apiService.Tell(new InjectActor<LaureateManagerActor>(laureateManager));
        });
    }

    private async Task FindYearSpan(YearSpan span)
    {
        YearSpan missingSpan;

        if (!HasYearSpan(span, out missingSpan))
            await FetchMissing(missingSpan);

        logger.Write($"Calculating average prize amount for years {span.From}-{span.To}.");
        var prizeAmountAdjusted = await prizeManager.Ask<float>(span);

        logger.Write($"Collection laureates for years {span.From}-{span.To}.");
        var laureates = await laureateManager.Ask<List<Laureate>>(span);

        logger.Write($"Replying to sender with collected data!");
        Sender.Tell(new Result(prizeAmountAdjusted, laureates));
    }

    private async Task FetchMissing(YearSpan span)
    {
        logger.Write($"Fetching missing year span {span.From}-{span.To}");
        
        await apiService.Ask(span);
        loadedSpans.Add(span);
    }

    private bool HasYearSpan(YearSpan span, out YearSpan result)
    {
        var existing = span.FindOverlap(loadedSpans);

        if (existing != null)
        {
            result = existing;
            return true;
        }

        result = span.Difference(loadedSpans);
        return false;
    }
}