using Ironyx.Kernel.Abstraction.Interfaces;

namespace Ironyx.Kernel.Options
{
    public class RequestOptions<TRequest>
        where TRequest : IRequest
    {
        public IList<Type> PreHandlers { get; } = [];
    }
}
