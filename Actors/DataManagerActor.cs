namespace SysProg.Actors;

public class DataManagerActor : ReceiveActor
{
    public record Result(float AveragePriceAmountAdjusted, List<Laureate> Laureates);

    private IActorRef prizeManager;
    private IActorRef laureateManager;

    private Logger logger;

    public DataManagerActor(Logger logger)
    {
        this.logger = logger;
        prizeManager = Context.ActorOf(Akka.Actor.Props.Create<PrizeManagerActor>(), "PrizeManager");
        laureateManager = Context.ActorOf(Akka.Actor.Props.Create<LaureateManagerActor>(), "LaureateManager");

        ReceiveAsync<YearSpan>(span => FindYearSpan(span));
        Receive<Prize>(prize => prizeManager.Forward(prize));
        Receive<Laureate>(prize => laureateManager.Forward(prize));
    }

    private async Task FindYearSpan(YearSpan span)
    {
        logger.Write($"Calculating average prize amount for years {span.From}-{span.To}.");
        var prizeAmountAdjusted = await prizeManager.Ask<float>(span);

        logger.Write($"Collection laureates for years {span.From}-{span.To}.");
        var laureates = await laureateManager.Ask<List<Laureate>>(span);

        logger.Write($"Replying to sender with collected data!");
        Sender.Tell(new Result(prizeAmountAdjusted, laureates));
    }

    public static Props Props(Logger logger) =>
        Akka.Actor.Props.Create(() => new DataManagerActor(logger));
}