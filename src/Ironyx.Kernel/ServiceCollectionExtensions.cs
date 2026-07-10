using Ironyx.Kernel.Execution.Dispatchers;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCommandDispatcher(this IServiceCollection services)
        {
            services.AddTransient<ICommandDispatcher, CommandDispatcher>();
        }
    }
}
