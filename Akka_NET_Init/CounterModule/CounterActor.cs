using System.Collections.Immutable;
using Akka.Actor;
using Akka.Event;
using static Akka_NET_Init.CounterModule.CounterCommands;
using static Akka_NET_Init.CounterModule.CounterQueries;

namespace Akka_NET_Init.CounterModule;

public sealed class CounterActor : UntypedActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<string, int> _wordCounts = new ();
    private bool _doneCounting = false;
    private readonly HashSet<IActorRef> _subscribers = new();
    
    protected override void OnReceive(object message)
    {
        switch (message)
        {
            case CountTokens countTokens:
            {
                foreach (var t in countTokens.Tokens)
                {
                    if (!_wordCounts.TryAdd(t, 1))
                    {
                        _wordCounts[t] += 1;
                    }
                }

                break;
            }
            case ExpectNoMoreTokens:
            {
                _log.Info($"Received ExpectNoMoreTokens - total tokens: {_wordCounts.Count}");
                _doneCounting = true;

                var totals = _wordCounts.ToImmutableDictionary();
                foreach (var s in _subscribers)
                {
                    s.Tell(totals);
                }
                _subscribers.Clear();
                break;
            }
            case FetchCounts fetchCounts when _doneCounting:
            {
                _log.Info($"Received FetchCounts - sending[{{_wordCounts.Count}}] to [{fetchCounts.Subscriber}]");
                fetchCounts.Subscriber.Tell(_wordCounts.ToImmutableDictionary());
                break;
            }
            case FetchCounts fetchCounts:
            {
                _log.Info($"Received FetchCounts - {fetchCounts.Subscriber}");
                _subscribers.Add(fetchCounts.Subscriber);
                break;
            }
            default:
                Unhandled(message);
                break;
        }
    }
}