using Ironyx.Kernel.Execution.Dispatchers;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Ironyx.Kernel
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static void AddCommandDispatcher(this IServiceCollection services)
        {
            services.AddTransient<ICommandDispatcher, CommandDispatcher>();
        }

        public static void AddQueryDispatcher(this IServiceCollection services)
        {
            services.AddTransient<IQueryDispatcher, QueryDispatcher>();
        }
    }
}
