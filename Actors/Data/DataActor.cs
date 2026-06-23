namespace SysProg.Actors.Data;

public class DataActor<T> : ReceiveActor
{
    public record Get;
    private T Item;

    public DataActor(T item)
    {
        Item = item;
        Receive<Get>(_ => Sender.Tell(Item));
    }

    public static Props Props(T Item) =>
        Akka.Actor.Props.Create(() => new DataActor<T>(Item));
} 
