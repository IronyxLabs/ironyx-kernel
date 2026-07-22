using Ironyx.Kernel.Abstraction.Interfaces;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public interface ICommandDispatcher
    {
        Task DispatchAsync(ICommand command, CancellationToken cancellationToken);
    }
}
