namespace SysProg;

using System.Reactive.Linq;
using System.Text.Json.Nodes;

public static class ObservableJsonExtensions
{
    public static IObservable<T> WhereNotNull<T>(this IObservable<T?> source) where T : class
    {
        return source
            .Where(item => item != null)
            .Select(item => item!);
    }

    public static IObservable<JsonNode> SelectManyJsonArray(
        this IObservable<JsonNode> source,
        Func<JsonNode, JsonNode?> arraySelector)
    {
        return source
            .Select(parent => arraySelector(parent) as JsonArray)
            .WhereNotNull()
            .SelectMany(parent => parent)
            .WhereNotNull();
    }

    public static IObservable<TResult> SelectManyJsonArray<TParent, TResult>(
        this IObservable<TParent> source,
        Func<TParent, JsonNode?> arraySelector,
        Func<TParent, JsonNode, TResult> resultSelector)
    {
        return source
            .SelectMany(
                parent => (arraySelector(parent) as JsonArray) ?? new JsonArray(),
                (parent, childNode) => resultSelector(parent, childNode!)
            );
    }
}
