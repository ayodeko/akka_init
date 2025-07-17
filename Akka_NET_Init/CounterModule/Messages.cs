using Akka.Actor;

namespace Akka_NET_Init.CounterModule;

public static class DocumentCommands
{
    public sealed record ProcessDocument(string RawText);
}

public static class CounterCommands
{
    public sealed record CountTokens(IReadOnlyList<string> Tokens);

    public sealed record ExpectNoMoreTokens();
}

public static class CounterQueries
{
    public sealed record FetchCounts(IActorRef Subscriber);
}