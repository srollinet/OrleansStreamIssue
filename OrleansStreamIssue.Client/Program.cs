using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using OrleansStreamIssue.Contracts;
using System;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;
using OrleansStreamIssue.Contracts.Data;

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

                //var streamProvider = host.GetStreamProvider(Constants.StreamProviderName);
                //var stream = streamProvider.GetStream<SimpleEvent>(Guid.Empty, nameof(SimpleEvent));
                //var observer = new StreamObserver(_logger);

                //var handle = await stream.SubscribeAsync(observer);
                //var resumeTimer = new Timer(async state =>
                //{
                //    try
                //    {
                //        await host.GetGrain<IIsAliveGrain>(0).IsAlive();
                //        _logger.LogInformation("Server is alive!");

                //        handle = await handle.ResumeAsync(observer);
                //        _logger.LogInformation("Stream subscription has been resumed!");
                //    }
                //    catch (Exception)
                //    {
                //        _logger.LogError("Server is not alive!");
                //    }
                //}, null,
                //TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                Console.WriteLine("Press Enter to terminate...");
                Console.ReadLine();

                //resumeTimer.Dispose();
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
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(IIsAliveGrain).Assembly).WithReferences())
                .AddClusterConnectionLostHandler((sender, args) =>
                {
                    _logger.LogError("Connection with the cluster is lost");
                })
                .ConfigureServices(services =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<SimpleEventConsumer>();
                        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            cfg.UseRetry(retry => retry.Interval(100, 200));
                            var host = cfg.Host(new Uri("rabbitmq://localhost"), h =>
                            {
                                h.Username("rabbitmq");
                                h.Password("rabbitmq");
                            });

                            cfg.ReceiveEndpoint(host, ep =>
                            {
                                ep.ConfigureConsumer<SimpleEventConsumer>(provider);
                                EndpointConvention.Map<SimpleEvent>(ep.InputAddress);
                            });
                        }));
                    });
                })
                .Build();

            _logger = client.ServiceProvider.GetService<ILogger<Program>>();

            await client.Connect(RetryFilter);

            var busControl = client.ServiceProvider.GetService<IBusControl>();
            await busControl.StartAsync();

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
