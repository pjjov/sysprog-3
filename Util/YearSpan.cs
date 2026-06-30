using System.Collections.Specialized;

namespace SysProg.Util;

public record YearSpan(int From, int To)
{
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
