using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EventStore.ClientAPI;
using EventSourcingTaskApp.HostedServices;

namespace HelloEventWorker
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
                    var eventStore = hostContext.Configuration.GetSection("EventStore");
                    var eventStoreConnection = EventStoreConnection.Create(
                        connectionString: eventStore["ConnectionString"],
                        builder: ConnectionSettings.Create().KeepReconnecting(),
                        connectionName: eventStore["ConnectionName"]);

                    eventStoreConnection.ConnectAsync().GetAwaiter().GetResult();

                    services.AddSingleton(eventStoreConnection);
                    services.AddHostedService<TaskHostedService>();
                });
    }
}
