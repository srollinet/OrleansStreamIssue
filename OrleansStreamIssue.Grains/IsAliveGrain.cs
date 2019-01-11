using System.Threading.Tasks;
using Orleans;
using OrleansStreamIssue.Contracts;

namespace OrleansStreamIssue.Grains
{
    class IsAliveGrain : Grain, IIsAliveGrain
    {
        public Task IsAlive()
        {
            return Task.CompletedTask;
        }
    }
}
