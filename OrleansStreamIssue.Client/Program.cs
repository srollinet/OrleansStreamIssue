using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansStreamIssue.Contracts;
using OrleansStreamIssue.Contracts.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OrleansStreamIssue.Client
{
    public class Program
    {
        private static readonly TimeSpan ConnectionRetryTime = TimeSpan.FromSeconds(2);

        private static ILogger _logger;

        public static Task Main()
        {
            return RunMainAsync();
        }

        private static async Task<int> RunMainAsync()
        {
            try
            {
                var host = await StartClient();

                var streamProvider = host.GetStreamProvider(Constants.StreamProviderName);
                var stream = streamProvider.GetStream<SimpleEvent>(Guid.Empty, nameof(SimpleEvent));
                var observer = new StreamObserver(_logger);

                var handle = await stream.SubscribeAsync(observer);
                var resumeTimer = new Timer(async state =>
                {
                    handle = await handle.ResumeAsync(observer);
                }, null,
                TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                resumeTimer.Dispose();
                await host.Close();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<IClusterClient> StartClient()
        {
            var client = new ClientBuilder().Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Constants.ClusterId;
                    options.ServiceId = Constants.ServiceId;
                })
                .UseLocalhostClustering()
                .ConfigureLogging(logging => logging.AddConsole())
                .AddSimpleMessageStreamProvider(Constants.StreamProviderName)
                .Build();

            _logger = client.ServiceProvider.GetService<ILogger<Program>>();

            await client.Connect(RetryFilter);

            return client;
        }

        /// <summary>
        /// Retries to connect perpetually
        /// </summary>
        private static async Task<bool> RetryFilter(Exception exception)
        {
            _logger.LogWarning(exception,
                $"Error while attempting to connect to Orleans cluster. Retry in {ConnectionRetryTime.TotalSeconds} seconds");
            await Task.Delay(ConnectionRetryTime);
            return true;
        }
    }
}
