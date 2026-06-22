using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Prize(int id, int year, PrizeCategory category, int prizeAmountAdjusted)
{
    public static Prize Parse(JsonNode data)
    {
        var categoryName = data["category"]?["en"]?.GetValue<string>();
        var awardYear = data["awardYear"]?.GetValue<int>() ?? 0;
        var prizeAmountAdjusted = data["prizeAmountAdjusted"]?.GetValue<int>();

        PrizeCategory category;
        if (!Enum.TryParse(categoryName, out category))
            category = PrizeCategory.Unknown;

        var id = 10 * awardYear + (int)category;

        return new Prize(
            id,
            awardYear,
            category,
            prizeAmountAdjusted ?? 0
        );
    }
}