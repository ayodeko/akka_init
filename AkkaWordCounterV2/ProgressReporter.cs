using Akka.Actor;
using Akka.Event;

namespace AkkaWordCounterV2;

public class ProgressReporter : ReceiveActor
{
    private readonly ILoggingAdapter _log = Context.GetLogger();
    private readonly Dictionary<string, bool> _urlStatus = new();

    public ProgressReporter()
    {
        Receive<ProgressReport>(msg =>
        {
            _urlStatus[msg.Url] = msg.IsComplete;
            _log.Info("Progress: {0} - {1} words processed, Complete: {2}", 
                     msg.Url, msg.WordsProcessed, msg.IsComplete);
        });

        Receive<WordCountComplete>(msg =>
        {
            _log.Info("=== WORD COUNT COMPLETE ===");
            _log.Info("Total unique words found: {0}", msg.TotalWordCounts.Count);
            
            // Show top 10 most frequent words
            var topWords = msg.TotalWordCounts
                             .OrderByDescending(kvp => kvp.Value)
                             .Take(10);
            
            _log.Info("Top 10 most frequent words:");
            foreach (var word in topWords)
            {
                _log.Info("  {0}: {1}", word.Key, word.Value);
            }
            
            // Note is to always terminate the actor system when work is complete
            Context.System.Terminate();
        });
    }
} 