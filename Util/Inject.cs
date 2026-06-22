namespace SysProg.Util;

public record Inject<T>(T Item);
public record InjectActor<T>(IActorRef Reference);