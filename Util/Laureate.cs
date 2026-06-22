using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Laureate(int Id, string Fullname, PrizeCategory Category, int PrizeYear)
{
    public static Laureate Parse(JsonNode parent, JsonNode data)
    {
        var id = data["id"]?.GetValue<int>();
        var categoryName = parent["category"]?["en"]?.GetValue<string>();
        var awardYear = parent["awardYear"]?.GetValue<int>() ?? 0;
        var fullName = data["fullName"]?.GetValue<string>();

        PrizeCategory category;
        if (!Enum.TryParse(categoryName, out category))
            category = PrizeCategory.Unknown;

        return new Laureate(
            id ?? -1,
            fullName ?? "Unknown",
            category,
            awardYear
        );
    }
}