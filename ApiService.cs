namespace SysProg;

using System.Reactive.Linq;
using System.Text.Json.Nodes;

public class ApiService
{
    private HttpClient _client;
    private Logger _logger;

    private const string url = "https://api.nobelprize.org/2.1/nobelPrizes?limit=1000";

    public IObservable<Prize> PrizeStream;
    public IObservable<Laureate> LaureateStream;

    public ApiService(Logger logger)
    {
        _client = new();
        _logger = logger;

        var dataStream = Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromMinutes(5))
            .SelectMany(_ => Observable.FromAsync(Query))
            .WhereNotNull();

        var baseStream = dataStream
            .SelectManyJsonArray(data => data["nobelPrizes"])
            .Publish()
            .RefCount();

        var prizeStream = baseStream
            .Select(prize => Prize.Parse(prize!));

        var laureateStream = baseStream.SelectManyJsonArray(
            prize => prize["laureates"],
            (prize, laureate) => Laureate.Parse(prize, laureate)
        );

        PrizeStream = prizeStream;
        LaureateStream = laureateStream;
    }

    private async Task<JsonNode?> Query()
    {
        try
        {
            _logger.Write($"Fetching data from API.");
            return await Fetch(url);
        }
        catch (Exception e)
        {
            _logger.Write(e);
            return null;
        }
    }

    private async Task<JsonNode> Fetch(string url)
    {
        HttpResponseMessage response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.Write($"Parsing data from API.");
        return JsonNode.Parse(responseBody)!;
    }
}