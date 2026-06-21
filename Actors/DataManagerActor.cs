namespace SysProg.Actors;

public class DataManagerActor: ReceiveActor
{
    private IActorRef prizeManager;
    private IActorRef laureateManager;
    private IActorRef apiService = ActorRefs.Nobody;
    private List<YearSpan> loadedSpans;

    public DataManagerActor()
    {
        loadedSpans = new ();
        prizeManager = Context.ActorOf(Akka.Actor.Props.Create<PrizeManagerActor>(), "PrizeManager");
        laureateManager = Context.ActorOf(Akka.Actor.Props.Create<LaureateManagerActor>(), "LaureateManager");

        ReceiveAsync<YearSpan>(span => FindYearSpan(span));
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
    }

    private async Task FetchMissing(YearSpan span)
    {
        await apiService.Ask(span);
        loadedSpans.Add(span);
    }

    private bool HasYearSpan(YearSpan span, out YearSpan result)
    {
        var existing = loadedSpans.FirstOrDefault(s =>
            span.From >= s.From &&
            span.To <= s.To);

        if (existing is not null)
        {
            result = existing;
            return true;
        }

        var overlaps = loadedSpans
            .Where(s => s.From <= span.To &&
                        s.To >= span.From)
            .ToList();

        if (overlaps.Count == 0)
        {
            result = span;
            return false;
        }

        result = new YearSpan(
            Math.Min(span.From, overlaps.Min(s => s.From)),
            Math.Max(span.To, overlaps.Max(s => s.To))
        );
        return false;
    }
}