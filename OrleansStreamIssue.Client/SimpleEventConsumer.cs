using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using OrleansStreamIssue.Contracts.Data;

namespace OrleansStreamIssue.Client
{
    public class SimpleEventConsumer : IConsumer<SimpleEvent>
    {
        private readonly ILogger<SimpleEventConsumer> _logger;

        public SimpleEventConsumer(ILogger<SimpleEventConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SimpleEvent> context)
        {
            var item = context.Message;
            _logger.LogInformation("Received event {eventId} with value {eventValue}", item.EventId, item.Value);
            return Task.CompletedTask;
        }
    }
}
