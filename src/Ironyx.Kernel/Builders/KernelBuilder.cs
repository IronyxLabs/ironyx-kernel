using Ironyx.Kernel.Execution;
using Ironyx.Kernel.Execution.Registries;
using Ironyx.Kernel.Execution.Senders;
using Ironyx.Kernel.Registry;
using Ironyx.Kernel.Senders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel.Builders
{
    public class KernelBuilder
    {
        private readonly WebApplicationBuilder _builder;
        private readonly CanonicalTypeRegistry _canonicalTypeRegistry;
        private readonly HandlerRegistry _handlerRegistry;

        public KernelBuilder(WebApplicationBuilder builder, CanonicalTypeRegistry canonicalTypeRegistry, HandlerRegistry handlerRegistry)
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

        public KernelBuilder AddQuery<TQuery, TResult, THandler>()
            where TQuery : Query<TResult>
            where THandler : class, IQueryHandler<TQuery, TResult>
        {
            _canonicalTypeRegistry.Add(typeof(TQuery));
            _handlerRegistry.Add(typeof(TQuery), typeof(THandler));

            _builder.Services.AddTransient<THandler>();

            return this;
        }

        public KernelBuilder AddCommandSender(Uri url)
        {
            _builder.Services.AddGrpcClient<GenericAPI.GenericAPIClient>(options => options.Address = url);
            _builder.Services.AddTransient<IRequestSender, GrpcRequestSender>();

            return this;
        }
    }
}
