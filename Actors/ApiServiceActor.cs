using System.Text.Json.Nodes;
using Akka.Actor;
using System.Reactive.Linq;
using SysProg.Util;

namespace SysProg.Actors;

public class ApiServiceActor: ReceiveActor
{
    private const string baseUrl = "https://api.nobelprize.org/2.1";
    private HttpClient client;
    private IActorRef laureateManager = ActorRefs.Nobody;
    private IActorRef prizeManager = ActorRefs.Nobody;

    public ApiServiceActor()
    {
        client = new ();
        client.BaseAddress = new Uri(baseUrl);
        
        ReceiveAsync<YearSpan>(Query);
        Receive<InjectActor<LaureateManagerActor>>(dep => laureateManager = dep.Reference);
        Receive<InjectActor<PrizeManagerActor>>(dep => prizeManager = dep.Reference);
    }

    private async Task Query(YearSpan span)
    {
        var data = await Fetch(YearSpanToUrl(span));

        var baseStream = Observable.Return(data)
            .SelectMany(data => (JsonArray)data["nobelPrizes"]!);
            
        var prizeStream = baseStream.Select(prize => Prize.Parse(data));

        var laureateStream = baseStream.SelectMany(
            prize => (JsonArray)prize!["laureates"]!,
            (prize, laureate) => Laureate.Parse(prize!, laureate!)
        );
        
        prizeStream.Subscribe(prize => prizeManager.Tell(prize));
        laureateStream.Subscribe(laureate => laureateManager.Tell(laureate));
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
        return $"/nobelPrizes?nobelPrizeYear={span.From}&yearTo={span.To}";
    }
}