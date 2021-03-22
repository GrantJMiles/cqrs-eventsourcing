namespace EventSourcingTaskApp.HostedServices
{
    using EventSourcingTaskApp.Core.Events;
    using EventStore.ClientAPI;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class TaskHostedService : IHostedService
    {
        private readonly IEventStoreConnection _eventStore;
        private readonly ILogger<TaskHostedService> _logger;

        private EventStoreAllCatchUpSubscription subscription;

        public TaskHostedService(IEventStoreConnection eventStore, ILogger<TaskHostedService> logger)
        {
            _eventStore = eventStore;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {

            var settings = new CatchUpSubscriptionSettings(
                maxLiveQueueSize: 10000,
                readBatchSize: 500,
                verboseLogging: false,
                resolveLinkTos: false,
                subscriptionName: "Tasks");

            subscription = _eventStore.SubscribeToAllFrom(
                lastCheckpoint: null,
                settings: settings,
                eventAppeared: (sub, @event) =>
                {
                    if (@event.OriginalEvent.EventType.StartsWith("$"))
                        return;

                    try
                    {
                        var str = Encoding.UTF8.GetString(@event.OriginalEvent.Metadata);
                        var ass = Assembly.Load("HelloWebApi");
                        var eventType = ass.GetType(str);
                        var eventData = JsonSerializer.Deserialize(Encoding.UTF8.GetString(@event.OriginalEvent.Data), eventType);

                        if (eventType != typeof(CreatedTask) && eventType != typeof(AssignedTask) && eventType != typeof(MovedTask) && eventType != typeof(CompletedTask))
                            return;

                        System.Console.WriteLine("**************************************************");
                        Console.WriteLine(eventData);
                        System.Console.WriteLine("**************************************************");
                        Console.WriteLine(Println(eventData));
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, exception.Message);
                    }
                },
                liveProcessingStarted: (sub) =>
                {
                    _logger.LogInformation("{SubscriptionName} subscription started.", sub.SubscriptionName);
                },
                subscriptionDropped: (sub, subDropReason, exception) =>
                {
                    _logger.LogWarning("{SubscriptionName} dropped. Reason: {SubDropReason}.", sub.SubscriptionName, subDropReason);
                });
        }
        private string Println(object oo) {
            return oo switch {
                AssignedTask x => $"{x.TaskId}: {x.AssignedTo} - {x.AssignedBy}",
                _ => "Not found"
            };
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            subscription.Stop();

            return Task.CompletedTask;
        }
    }
}