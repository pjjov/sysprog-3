using System.Net;
using Akka.Actor;

namespace SysProg.Actors;

public class DataManagerActor: ReceiveActor
{
    private IActorRef prizeManager;
    private IActorRef lauterateManager;
    private List<YearSpan> loadedSpans;

    public DataManagerActor()
    {
        loadedSpans = new ();
        prizeManager = Context.ActorOf(Akka.Actor.Props.Create<PrizeManagerActor>(), "prizes");
        lauterateManager = Context.ActorOf(Akka.Actor.Props.Create<LauterateManagerActor>(), "lauterates");

        ReceiveAsync<YearSpan>(span => FindYearSpan(span));
    }

    protected async Task FindYearSpan(YearSpan span)
    {
        YearSpan missingSpan;

        if (!HasYearSpan(span, out missingSpan))
            await FetchYearSpan(missingSpan);
    }

    protected async Task FetchYearSpan(YearSpan span)
    {
        loadedSpans.Add(span);
    }

    protected bool HasYearSpan(YearSpan span, out YearSpan result)
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