using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansStreamIssue.Contracts;
using OrleansStreamIssue.Contracts.Data;
using OrleansStreamIssue.Grains;

namespace OrleansStreamIssue.Server
{
    public class Program
    {
        public static Task<int> Main()
        {
            return RunMainAsync();
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartSilo();
                var counter = 0;

                var publishTimer = new Timer(async state =>
                {
                    await host.Services.GetService<IGrainFactory>().GetGrain<IEventHubGrain>(0)
                        .Publish(new SimpleEvent("Message " + counter++));
                }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                publishTimer.Dispose();
                await host.StopAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<ISiloHost> StartSilo()
        {
            // define the cluster configuration
            var builder = new SiloHostBuilder()
                .UseLocalhostClustering()
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Constants.ClusterId;
                    options.ServiceId = Constants.ServiceId;
                })
                .AddSimpleMessageStreamProvider(Constants.StreamProviderName)
                .AddMemoryGrainStorage("PubSubStore")
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(EventHubGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .EnableDirectClient();

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}