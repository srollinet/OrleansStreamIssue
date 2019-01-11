using System.Threading.Tasks;
using Orleans;
using OrleansStreamIssue.Contracts.Data;

namespace OrleansStreamIssue.Contracts
{
    public interface IEventHubGrain : IGrainWithIntegerKey
    {
        Task Publish(SimpleEvent evt);
    }
}
