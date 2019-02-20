﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using OrleansStreamIssue.Contracts;
using OrleansStreamIssue.Contracts.Data;
using OrleansStreamIssue.Grains;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GreenPipes;
using MassTransit;

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
                    try
                    {
                        await host.Services.GetService<IGrainFactory>().GetGrain<IEventHubGrain>(0)
                            .Publish(new SimpleEvent("Message " + counter++));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("oops....");
                    }
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
                .Configure<EndpointOptions>(options => options.AdvertisedIPAddress = IPAddress.Loopback)
                .Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = Constants.ClusterId;
                    options.ServiceId = Constants.ServiceId;
                })
                .ConfigureServices(services =>
                {
                    services.AddMassTransit(x =>
                    {
                        x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                        {
                            cfg.Host(new Uri("rabbitmq://localhost"), h =>
                            {
                                h.Username("rabbitmq");
                                h.Password("rabbitmq");
                            });
                        }));
                        x.AddRequestClient<SimpleEvent>();
                    });
                })
                .ConfigureApplicationParts(parts =>
                    parts.AddApplicationPart(typeof(EventHubGrain).Assembly).WithReferences())
                .ConfigureLogging(logging => logging.AddConsole())
                .EnableDirectClient();

            var host = builder.Build();
            await host.StartAsync();
            return host;
        }
    }
}