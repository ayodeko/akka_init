using Akka.Actor;
using Akka.Hosting;
using AkkaWordCounterV2;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var hostBuilder = new HostBuilder();

hostBuilder.ConfigureServices((context, services) =>
{
    services.AddAkka("WordCounterSystem", (builder, sp) =>
    {
        
        builder
            .WithActors((system, registry, resolver) =>
            {
                // Create the progress reporter actor
                var reporter = system.ActorOf(Props.Create<ProgressReporter>(), "progress-reporter");
                registry.Register<ProgressReporter>(reporter);
            })
            .WithActors((system, registry, resolver) =>
            {
                // Get the reporter from the registry
                var reporter = registry.Get<ProgressReporter>();
                
                // Sample URLs to process
                var urls = new[]
                {
                    "https://petabridge.com/blog/",
                    "https://getakka.net/",
                    "https://github.com/akkadotnet/akka.net"
                };
                
                // Create the coordinator actor
                var coordinator = system.ActorOf(
                    Props.Create(() => new WordCountCoordinator(urls, reporter)), 
                    "word-count-coordinator");
                
                // Start the word counting process
                coordinator.Tell(new StartWordCount(urls));
            });
    });
});

var host = hostBuilder.Build();

await host.RunAsync();