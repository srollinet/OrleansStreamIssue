using Microsoft.Extensions.Logging;
using Orleans.Streams;
using OrleansStreamIssue.Contracts.Data;
using System;
using System.Threading.Tasks;

namespace OrleansStreamIssue.Client
{

    public class StreamObserver : IAsyncObserver<SimpleEvent>
    {
        private readonly ILogger _logger;

        public StreamObserver(ILogger logger)
        {
            _logger = logger;
        }

        public Task OnCompletedAsync()
        {
            _logger.LogInformation("Received stream completed event");
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            _logger.LogError(ex, "Experiencing message delivery failure");
            return Task.CompletedTask;
        }

        public Task OnNextAsync(SimpleEvent item, StreamSequenceToken token = null)
        {
            _logger.LogInformation("Received event {eventId} with value {eventValue}", item.EventId, item.Value);
            return Task.CompletedTask;
        }
    }
}
