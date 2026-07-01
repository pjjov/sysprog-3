namespace SysProg;

using System.Reactive.Linq;
using System.Text.Json.Nodes;

public class ApiService
{
    private HttpClient _client;
    private Logger _logger;

    private int _limit;
    private int _offset = 0;
    private int _count = 1000;

    private const string baseUrl = "https://api.nobelprize.org/2.1/nobelPrizes";

    public IObservable<Prize> PrizeStream;
    public IObservable<Laureate> LaureateStream;

    public ApiService(Logger logger, int limit = 25)
    {
        _client = new();
        _logger = logger;
        _limit = limit;

        var dataStream = Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(5))
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
        if (_offset >= _count)
            return null;

        try
        {
            _logger.Write($"Fetching data from API. ({_offset}-{_offset + _limit})");
            var result = await Fetch(MakeUrl());

            _offset += _limit;
            return result;
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

    private string MakeUrl()
    {
        return $"{baseUrl}?limit={_limit}&offset={_offset}";
    }
}