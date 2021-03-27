using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MassTransit;
using Abstractions;
using EventStore.ClientAPI;

namespace ConsumerOne
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<CreateEmailEventConsumer>();
                        x.UsingRabbitMq((context, cfg) =>
                        {
                            cfg.Host("masstransit");
                            
                            cfg.ConfigureEndpoints(context);

                        });
                    });
                    var eventStore = hostContext.Configuration.GetSection("EventStore");
                    var eventStoreConnection = EventStoreConnection.Create(
                        connectionString: eventStore["ConnectionString"],
                        builder: ConnectionSettings.Create().KeepReconnecting(),
                        connectionName: eventStore["ConnectionName"]);

                    eventStoreConnection.ConnectAsync().GetAwaiter().GetResult();

                    services.AddSingleton(eventStoreConnection);
                    services.AddTransient<AggregateRepository>();
                    services.AddMassTransitHostedService();
                    
                });
    }
    class CreateEmailEventConsumer : IConsumer<CreateEmail>
    {
        ILogger<CreateEmailEventConsumer> _logger;
        IEventStoreConnection _eventStore;

        public CreateEmailEventConsumer(ILogger<CreateEmailEventConsumer> logger, IEventStoreConnection eventStore)
        {
            _logger = logger;
            _eventStore = eventStore;
        }

        public async Task Consume(ConsumeContext<CreateEmail> context)
        {
            _logger.LogInformation($"Email has been created and sent to be processed... {context.Message.Title} - {context.Message.Recipient}");
            var agg = await _eventStore.
        }
    }
}
