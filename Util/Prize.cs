using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Prize(int year, PrizeCategory category, int prizeAmountAdjusted)
{
    public static Prize Parse(JsonNode data)
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
}