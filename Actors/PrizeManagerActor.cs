using Akka.Actor;
using SysProg.Util;

namespace SysProg.Actors;

public record Prize(int year, PrizeCategory category, int prizeAmountAdjusted);

public class PrizeManagerActor: ReceiveActor
{
    
}