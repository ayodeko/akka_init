using System.Collections.Immutable;

namespace AkkaWordCounterV2;

// Messages for the word counter system
public record StartWordCount(string[] Urls);
public record ProcessUrl(string Url);
public record UrlProcessed(string Url, Dictionary<string, int> WordCounts);
public record WordCountComplete(Dictionary<string, int> TotalWordCounts);
public record ProgressReport(string Url, int WordsProcessed, bool IsComplete);
public record TimeoutMessage; 