using System;

namespace OrleansStreamIssue.Contracts.Data
{
    public class SimpleEvent
    {
        public Guid EventId { get; }

        public string Value { get; }

        public SimpleEvent(string value)
        {
            EventId = Guid.NewGuid();
            Value = value;
        }
    }
}
