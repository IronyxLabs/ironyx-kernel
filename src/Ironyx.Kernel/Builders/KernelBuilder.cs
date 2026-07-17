using Ironyx.Kernel.Registry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel.Builders
{
    public class KernelBuilder
    {
        private readonly WebApplicationBuilder _builder;
        private readonly CanonicalTypeRegistry _canonicalTypeRegistry;

        public KernelBuilder(WebApplicationBuilder builder, CanonicalTypeRegistry canonicalTypeRegistry)
        {
            _builder = builder;
            _canonicalTypeRegistry = canonicalTypeRegistry;
        }

        public KernelBuilder AddHandler<TCommand, TCommandHandler>()
            where TCommand : Command
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            _builder.Services.AddTransient<ICommandHandler<TCommand>, TCommandHandler>();

            return this;
        }

        public KernelBuilder AddCommand<TCommand>()
            where TCommand : Command
        {
            _canonicalTypeRegistry.Add(typeof(TCommand));

            return this;
        }
    }
}
