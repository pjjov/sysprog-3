public record Logger(IActorRef actor) {
    public static Logger Nobody = new Logger(ActorRefs.Nobody);
    public void Write<T>(T msg) {
        actor.Tell(new LoggerActor.Write<T>(msg));
    }
};