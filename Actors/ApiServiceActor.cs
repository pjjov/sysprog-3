using System.Text.Json;
using System.Text.Json.Nodes;
using Akka.Actor;

namespace SysProg.Actors;

public class ApiServiceActor: ReceiveActor
{
    private const string baseUrl = "https://api.nobelprize.org/2.1";
    private HttpClient client;

    public ApiServiceActor()
    {
        client = new ();
        client.BaseAddress = new Uri(baseUrl);
        ReceiveAsync<YearSpan>(Query);
    }

    protected async Task Query(YearSpan span)
    {
        var data = Fetch(YearSpanToUrl(span));
        // Rx.Net parsing
    }

    protected async Task<JsonNode> Fetch(string url)
    {   
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();
        return JsonNode.Parse(responseBody)!;
    }

    protected string YearSpanToUrl(YearSpan span)
    {
        return $"/nobelPrizes?nobelPrizeYear={span.From}&yearTo={span.To}";
    }
}