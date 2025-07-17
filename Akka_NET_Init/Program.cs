// See https://aka.ms/new-console-template for more information

using Akka_NET_Init;
using Akka.Actor;
using Akka.Event;

ActorSystem myActorSystem = ActorSystem.Create("actorSystem");

//Actor creator option definition
var helloActorProps = Props.Create(() => new HelloActor());

var helloActor = myActorSystem.ActorOf(helloActorProps, "helloActor");

helloActor.Tell("Hello from hello actor"); // WIll produce dead letter since there are no receivers;
string response = await helloActor.Ask<string>("Hello from hello actor", TimeSpan.FromSeconds(2));
Console.Write($"Response from hello actor: {response}");
myActorSystem.Log.Info("Hello");
await myActorSystem.Terminate();