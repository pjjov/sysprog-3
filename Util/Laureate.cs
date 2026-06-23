using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Laureate(int Id, string Fullname, string Category, int PrizeYear)
{
    public static Laureate Parse(JsonNode parent, JsonNode data)
    {
        var id = int.Parse(data["id"]?.GetValue<string>() ?? "-1");
        var category = parent["category"]?["en"]?.GetValue<string>();
        var awardYear = int.Parse(parent["awardYear"]?.GetValue<string>() ?? "0");
        var fullName = data["fullName"]?["en"]?.GetValue<string>();

        return new Laureate(
            id,
            fullName ?? "Unknown",
            category ?? "Unknown",
            awardYear
        );
    }
}