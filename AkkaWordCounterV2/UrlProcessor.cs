using Akka.Actor;
using Akka.Event;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace AkkaWordCounterV2;

public class UrlProcessor : ReceiveActor, IWithTimers
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly string _url;
    private readonly IActorRef _reporter;
    private readonly HttpClient _httpClient;
    private readonly IActorRef _coordinator;

    public ITimerScheduler Timers { get; set; } = null!;

    public UrlProcessor(string url, IActorRef reporter)
    {
        _url = url;
        _reporter = reporter;
        _httpClient = new HttpClient();
        _coordinator = Context.Parent;

        //Use ReceiveTimeout to terminate idle actors
        Context.SetReceiveTimeout(TimeSpan.FromMinutes(5));

        Receive<ProcessUrl>(_ => ProcessUrlAsync());
        Receive<ReceiveTimeout>(_ => HandleTimeout());
    }

    private async void ProcessUrlAsync()
    {
        try
        {
            _log.Info("Processing URL: {0}", _url);
            
            //Handle Task inside actors
            var content = await _httpClient.GetStringAsync(_url);
            var wordCounts = CountWords(content);
            
            _reporter.Tell(new ProgressReport(_url, wordCounts.Count, true));
            _coordinator.Tell(new UrlProcessed(_url, wordCounts));
            
            // Terminate this actor after processing
            Context.Self.Tell(PoisonPill.Instance);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Error processing URL: {0}", _url);
            _reporter.Tell(new ProgressReport(_url, 0, true));
            Context.Self.Tell(PoisonPill.Instance);
        }
    }

    private Dictionary<string, int> CountWords(string content)
    {
        
        var cleanContent = Regex.Replace(content, "<[^>]*>", " ");
        var words = Regex.Split(cleanContent.ToLower(), @"\W+")
                        .Where(w => w.Length > 2)
                        .Where(w => !string.IsNullOrWhiteSpace(w));

        var wordCounts = new Dictionary<string, int>();
        foreach (var word in words)
        {
            if (wordCounts.ContainsKey(word))
                wordCounts[word]++;
            else
                wordCounts[word] = 1;
        }

        return wordCounts;
    }

    private void HandleTimeout()
    {
        _log.Warning("Timeout processing URL: {0}", _url);
        _reporter.Tell(new ProgressReport(_url, 0, true));
        Context.Self.Tell(PoisonPill.Instance);
    }

    protected override void PostStop()
    {
        _httpClient?.Dispose();
        base.PostStop();
    }
} 