using System.Collections.Specialized;

namespace SysProg.Util;

public record YearSpan(int From, int To)
{
    public YearSpan? FindOverlap(List<YearSpan> spans)
    {
        var existing = spans.FirstOrDefault(s =>
            From >= s.From &&
            To <= s.To
        );

        return existing;
    }

    public YearSpan Difference(List<YearSpan> spans)
    {
        var overlaps = spans
            .Where(s => s.From <= To && s.To >= From)
            .ToList();

        if (overlaps.Count == 0)
            return this;

        return new YearSpan(
            Math.Min(From, overlaps.Min(s => s.From)),
            Math.Max(To, overlaps.Max(s => s.To))
        );
    }

    public static YearSpan ParseQuery(NameValueCollection query)
    {
        var fromParam = query["from"];
        var toParam = query["to"];

        if (fromParam == null || toParam == null)
            throw new Exception("Both 'from' and 'to' query parameters must be specified!");

        int from, to;

        if (!int.TryParse(fromParam, out from) || !int.TryParse(toParam, out to))
            throw new Exception("Query parameters must be numbers");

        if (from > to)
            throw new Exception("Invalid year span");

        return new YearSpan(from, to);
    }
}
