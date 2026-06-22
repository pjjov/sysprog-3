public class DataActor<T> : UntypedActor
{
    private T Item;

    public DataActor(T item)
    {
        Item = item;
    }

    protected override void OnReceive(object message)
    {
        Sender.Tell(Item);
    }

    public static Props Props(T Item) =>
        Akka.Actor.Props.Create(() => new DataActor<T>(Item));
} 
