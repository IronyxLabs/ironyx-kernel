using Ironyx.Kernel.Abstraction.Interfaces;

namespace Ironyx.Kernel.Execution.Behaviors
{
    public interface IPreHandler<TRequest>
        where TRequest : IRequest
    {
        Task HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}
