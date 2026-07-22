using Ironyx.Kernel.Execution.Registries;
using Ironyx.Kernel.Registry;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel.Builders
{
    public class KernelBuilder
    {
        private readonly WebApplicationBuilder _builder;
        private readonly CanonicalTypeRegistry _canonicalTypeRegistry;
        private readonly CommandHandlerRegistry _handlerRegistry;

        public KernelBuilder(WebApplicationBuilder builder, CanonicalTypeRegistry canonicalTypeRegistry, CommandHandlerRegistry handlerRegistry)
        {
            _builder = builder;
            _canonicalTypeRegistry = canonicalTypeRegistry;
            _handlerRegistry = handlerRegistry;
        }

        public KernelBuilder AddCommand<TCommand, THandler>()
            where TCommand : Command
            where THandler : class, ICommandHandler<TCommand>
        {
            _canonicalTypeRegistry.Add(typeof(TCommand));
            _handlerRegistry.Add(typeof(TCommand), typeof(THandler));

            _builder.Services.AddTransient<THandler>();

            return this;
        }

        public KernelBuilder AddQuery<TQuery, TResult>()
            where TQuery : Query<TResult>
        {
            _canonicalTypeRegistry.Add(typeof(TQuery));

            return this;
        }
    }
}
