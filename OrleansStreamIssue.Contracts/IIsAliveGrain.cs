using System.Threading.Tasks;
using Orleans;

namespace OrleansStreamIssue.Contracts
{
    public interface IIsAliveGrain : IGrainWithIntegerKey
    {
        Task IsAlive();
    }
}
