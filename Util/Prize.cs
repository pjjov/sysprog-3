using System.Text.Json.Nodes;

namespace SysProg.Util;

public record Prize(int id, int year, string category, int prizeAmountAdjusted)
{
    public static Random r = new();

    public static Prize Parse(JsonNode data)
    {
        var category = data["category"]?["en"]?.GetValue<string>();
        var awardYear = int.Parse(data["awardYear"]?.GetValue<string>() ?? "0");
        var prizeAmountAdjusted = data["prizeAmountAdjusted"]?.GetValue<int>();

        var id = awardYear + category?.GetHashCode() ?? 0;

        return new Prize(
            id,
            awardYear,
            category ?? "Unknown",
            prizeAmountAdjusted ?? 0
        );
    }
}