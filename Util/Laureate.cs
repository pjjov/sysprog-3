using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Laureate(string Fullname, PrizeCategory Category, int PrizeYear)
{
    public static Laureate Parse(JsonNode parent, JsonNode data)
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
}