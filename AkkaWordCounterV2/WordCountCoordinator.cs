using Akka.Actor;
using Akka.Event;

namespace AkkaWordCounterV2;

public class WordCountCoordinator : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<string, IActorRef> _urlWorkers = new();
    private readonly Dictionary<string, Dictionary<string, int>> _results = new();
    private readonly IActorRef _reporter;
    private int _pendingUrls;
    private readonly string[] _urls;

    public WordCountCoordinator(string[] urls, IActorRef reporter)
    {
        _urls = urls;
        _reporter = reporter;
        _pendingUrls = urls.Length;

        // Using switchable behaviors for state management
        Become(Processing);
    }

    private void Processing()
    {
        Receive<StartWordCount>(msg =>
        {
            _log.Info("Starting word count for {0} URLs", msg.Urls.Length);
            
            foreach (var url in msg.Urls)
            {
                var worker = Context.ActorOf(Props.Create(() => new UrlProcessor(url, _reporter)), $"url-processor-{Guid.NewGuid()}");
                _urlWorkers[url] = worker;
                worker.Tell(new ProcessUrl(url));
            }
        });

        Receive<UrlProcessed>(msg =>
        {
            _log.Info("Received results for {0}", msg.Url);
            _results[msg.Url] = msg.WordCounts;
            _pendingUrls--;

            if (_pendingUrls == 0)
            {
                // All URLs processed, aggregate results
                var totalCounts = AggregateResults();
                _reporter.Tell(new WordCountComplete(totalCounts));
                Context.Self.Tell(PoisonPill.Instance);
            }
        });
    }

    private Dictionary<string, int> AggregateResults()
    {
        var totalCounts = new Dictionary<string, int>();
        
        foreach (var result in _results.Values)
        {
            foreach (var kvp in result)
            {
                if (totalCounts.ContainsKey(kvp.Key))
                    totalCounts[kvp.Key] += kvp.Value;
                else
                    totalCounts[kvp.Key] = kvp.Value;
            }
        }

        return totalCounts;
    }
} 