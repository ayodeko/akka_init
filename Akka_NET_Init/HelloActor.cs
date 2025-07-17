using Akka.Actor;
using Akka.Event;

namespace Akka_NET_Init;

public class HelloActor : UntypedActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case string msg:
            {
                _log.Info($"Received message {msg}");
                Sender.Tell($"{msg} reply");
                break;
            }
            default:
            {
                Unhandled(message);
                break;
            }
        }
    }
}