// See https://aka.ms/new-console-template for more information

using Akka_NET_Init;
using Akka_NET_Init.CounterModule;
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



// Start CounterActor and ParserActor
Props counterActorProps = Props.Create(() => new CounterActor());
IActorRef counterActorRef = myActorSystem.ActorOf(counterActorProps, "counterActor");
Props parserActorProps = Props.Create(() => new ParserActor(counterActorRef));
IActorRef parserActor = myActorSystem.ActorOf(parserActorProps, "parserActor");

Task<IDictionary<string, int>> completionPromise = counterActorRef
    .Ask<IDictionary<string, int>>(@ref => new CounterQueries.FetchCounts(@ref),
        null, CancellationToken.None);

parserActor.Tell(new DocumentCommands.ProcessDocument("""
                                                      The quick fox jumped over the green fence
                                                      """));

IDictionary<string, int> wordCounts = await completionPromise;

foreach (var pair in wordCounts)
{
    Console.WriteLine($"{pair.Key}: {pair.Value} instances");
}
await myActorSystem.Terminate();