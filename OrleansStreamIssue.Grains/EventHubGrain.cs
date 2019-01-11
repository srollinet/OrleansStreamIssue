using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Streams;
using OrleansStreamIssue.Contracts;
using OrleansStreamIssue.Contracts.Data;

namespace OrleansStreamIssue.Grains
{
    public class EventHubGrain : Grain, IEventHubGrain
    {
        private IAsyncStream<SimpleEvent> _eventStream;
        private readonly ILogger<EventHubGrain> _logger;

        public EventHubGrain(ILogger<EventHubGrain> logger)
        {
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            var streamProvider = GetStreamProvider(Constants.StreamProviderName);
            _eventStream = streamProvider.GetStream<SimpleEvent>(Guid.Empty, nameof(SimpleEvent));
            return Task.CompletedTask;
        }

        public Task Publish(SimpleEvent evt)
        {
            _logger.LogInformation("Publishing event {eventId} with value {eventValue}", evt.EventId, evt.Value);
            return _eventStream.OnNextAsync(evt);
        }
    }
}
