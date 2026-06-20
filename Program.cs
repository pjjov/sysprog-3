using Akka.Actor;
using SysProg.Actors;

Console.WriteLine("Hello, World!");

var system = ActorSystem.Create("sysprog");
var app = system.ActorOf(Props.Create<App>(), "app");

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    system.Terminate();
};

await system.WhenTerminated;