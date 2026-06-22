using System.Text.Json.Nodes;
using Akka.Actor;
using System.Reactive.Linq;
using SysProg.Util;
using System.Reactive.Subjects;

namespace SysProg.Actors;

public class ApiServiceActor: ReceiveActor
{
    private HttpClient client;
    private IActorRef laureateManager = ActorRefs.Nobody;
    private IActorRef prizeManager = ActorRefs.Nobody;
    private Logger logger = Logger.Nobody;
    private Subject<JsonNode> dataStream;

    public ApiServiceActor()
    {
        dataStream = new ();
        InitStreams();

        client = new();

        ReceiveAsync<YearSpan>(Query);
        Receive<Inject<Logger>>(dep => logger = dep.Item);
        Receive<InjectActor<LaureateManagerActor>>(dep => laureateManager = dep.Reference);
        Receive<InjectActor<PrizeManagerActor>>(dep => prizeManager = dep.Reference);
    }

    private void InitStreams()
    {
        var baseStream = dataStream.SelectMany(data => (JsonArray)data["nobelPrizes"]!);
            
        var prizeStream = baseStream.Select(prize => Prize.Parse(prize!));

        var laureateStream = baseStream.SelectMany(
            prize => (JsonArray)prize!["laureates"]!,
            (prize, laureate) => Laureate.Parse(prize!, laureate!)
        );
        
        prizeStream.Subscribe(prize => prizeManager.Tell(prize));
        laureateStream.Subscribe(laureate => laureateManager.Tell(laureate));
    }

    private async Task Query(YearSpan span)
    {
        
        try
        {
            var url = YearSpanToUrl(span);
            logger.Write($"Fetching {url}");
            
            var data = await Fetch(url);
            dataStream.OnNext(data);
            Sender.Tell(true);
        }
        catch (Exception e)
        {
            logger.Write(e);
            Sender.Tell(false);
        }
    }

    protected override void PostStop()
    {
        dataStream.OnCompleted();
        dataStream.Dispose();
        base.PostStop();
    }

    public void Dispose()
    {
        dataStream.Dispose();
    }

    private async Task<JsonNode> Fetch(string url)
    {   
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(responseBody)!;
    }

    private string YearSpanToUrl(YearSpan span)
    {
        return $"https://api.nobelprize.org/2.1/nobelPrizes?nobelPrizeYear={span.From}&yearTo={span.To}";
    }
}