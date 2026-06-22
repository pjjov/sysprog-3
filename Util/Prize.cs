using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Prize(int id, int year, PrizeCategory category, int prizeAmountAdjusted)
{

    public static Random r = new();
    public static Prize Parse(JsonNode data)
    {
        var categoryName = data["category"]?["en"]?.GetValue<string>();
        var awardYear = int.Parse(data["awardYear"]?.GetValue<string>() ?? "0");
        var prizeAmountAdjusted = data["prizeAmountAdjusted"]?.GetValue<int>();

        PrizeCategory category;
        if (!Enum.TryParse(categoryName, out category))
            category = PrizeCategory.Unknown;

        // var id = 10 * awardYear + (int)category;
        var id = r.Next();

        return new Prize(
            id,
            awardYear,
            category,
            prizeAmountAdjusted ?? 0
        );
    }
}