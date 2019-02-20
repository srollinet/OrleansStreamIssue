using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansStreamIssue.Contracts;
using OrleansStreamIssue.Contracts.Data;

namespace OrleansStreamIssue.Grains
{
    public class EventHubGrain : Grain, IEventHubGrain
    {
        private readonly ILogger<EventHubGrain> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public EventHubGrain(ILogger<EventHubGrain> logger, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public override Task OnActivateAsync()
        {
            return Task.CompletedTask;
        }

        public Task Publish(SimpleEvent evt)
        {
            _logger.LogInformation("Publishing event {eventId} with value {eventValue}", evt.EventId, evt.Value);
            return _publishEndpoint.Publish(evt);
        }
    }
}
