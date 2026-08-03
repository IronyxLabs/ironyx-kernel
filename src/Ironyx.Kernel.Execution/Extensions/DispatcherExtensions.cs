using Ironyx.Kernel.Abstraction.Interfaces;

namespace Ironyx.Kernel.Execution.Extensions
{
    internal static class DispatcherExtensions
    {
        public static async Task InvokeAsync<TRequest>(this IEnumerable<Type> handlerTypes, TRequest request, IServiceProvider provider, CancellationToken cancellationToken)
            where TRequest : IRequest
        {
            foreach (var handlerType in handlerTypes)
            {
                await ((dynamic)provider.GetService(handlerType))!.HandleAsync((dynamic)request, cancellationToken);
            }
        }
    }
}
