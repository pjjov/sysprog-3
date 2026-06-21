using System.Text.Json.Nodes;
using Akka.Actor;
using System.Reactive.Linq;
using SysProg.Util;

namespace SysProg.Actors;

public class ApiServiceActor: ReceiveActor
{
    private const string baseUrl = "https://api.nobelprize.org/2.1";
    private HttpClient client;
    private IActorRef? laureateManager;
    private IActorRef? prizeManager;

    public ApiServiceActor()
    {
        client = new ();
        client.BaseAddress = new Uri(baseUrl);
        ReceiveAsync<YearSpan>(Query);
    }

    private async Task Query(YearSpan span)
    {
        var data = await Fetch(YearSpanToUrl(span));

        var baseStream = Observable.Return(data)
            .SelectMany(data => (JsonArray)data["nobelPrizes"]!);
            
        var prizeStream = baseStream.Select(prize => ParsePrize(data));

        var laureateStream = baseStream.SelectMany(
            prize => (JsonArray)prize!["laureates"]!,
            (prize, laureate) => ParseLaureate(prize!, laureate!)
        );

        var prizeManager = await GetPrizeManager();
        var laureateManager = await GetLaureateManager();
        
        prizeStream.Subscribe(prize => prizeManager.Tell(prize));
        laureateStream.Subscribe(laureate => laureateManager.Tell(laureate));
    }

    private async Task<IActorRef> GetLaureateManager()
    {
        if (laureateManager.IsNobody())
            laureateManager = await Context.ActorSelection("../DataManager/LaureateManager").ResolveOne(new TimeSpan(0, 5, 0));
        return laureateManager!;
    }

    private async Task<IActorRef> GetPrizeManager()
    {
        if (prizeManager.IsNobody())
            prizeManager = await Context.ActorSelection("../DataManager/PrizeManager").ResolveOne(new TimeSpan(0, 5, 0));
        return prizeManager!;
    }

    private Prize ParsePrize(JsonNode data)
    {
        var category = data["category"]?["en"]?.GetValue<string>();
        var awardYear = data["awardYear"]?.GetValue<string>();
        var prizeAmountAdjusted = data["prizeAmountAdjusted"]?.GetValue<int>();

        return new Prize(
            int.Parse(awardYear ?? "0"),
            Enum.Parse<PrizeCategory>(category ?? "Unknown"),
            prizeAmountAdjusted ?? 0
        );
    }

    private Laureate ParseLaureate(JsonNode parent, JsonNode data)
    {
        var category = parent["category"]?["en"]?.GetValue<string>();
        var awardYear = parent["awardYear"]?.GetValue<string>();
        var fullName = data["fullName"]?.GetValue<string>();

        return new Laureate(
            fullName ?? "Unknown",
            Enum.Parse<PrizeCategory>(category ?? "Unknown"),
            int.Parse(awardYear ?? "0")
        );
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