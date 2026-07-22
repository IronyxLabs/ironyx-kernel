using Ironyx.Kernel.Abstraction.Interfaces;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public interface IQueryDispatcher
    {
        Task<TResult> DispatchAsync<TResult>(IQuery query, CancellationToken cancellationToken);
    }
}
