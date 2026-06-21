using Akka.Actor;
using SysProg.Util;

namespace SysProg.Actors;
public record Laureate(string Fullname, PrizeCategory Category, int PrizeYear);
public class LaureateManagerActor: ReceiveActor
{
    
}