using Akka.Actor;
using Akka.Event;
using static Akka_NET_Init.CounterModule.CounterCommands;
using static Akka_NET_Init.CounterModule.DocumentCommands;

namespace Akka_NET_Init.CounterModule;

public sealed class ParserActor : UntypedActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly IActorRef _counterActor;
    
    /// <summary>
    /// Amount of token sent to counter actor at a time
    /// </summary>
    private const int TokenBatchSize = 10;

    public ParserActor(IActorRef counterActor)
    {
        _counterActor = counterActor;
    }
    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case ProcessDocument process:
            {
                foreach (var tokenBatch in process.RawText.Split(" ").Chunk(TokenBatchSize))
                {
                    _counterActor.Tell(new CountTokens(tokenBatch));
                }
                _counterActor.Tell(new ExpectNoMoreTokens());
                break;
            }
            default:
                Unhandled(message);
                break;
        }
    }
}