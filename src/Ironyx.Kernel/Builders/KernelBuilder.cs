using Ironyx.Kernel.Execution;
using Ironyx.Kernel.Execution.Behaviors;
using Ironyx.Kernel.Execution.Registries;
using Ironyx.Kernel.Execution.Senders;
using Ironyx.Kernel.Interceptors;
using Ironyx.Kernel.Options;
using Ironyx.Kernel.Registry;
using Ironyx.Kernel.Senders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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

        public KernelBuilder AddCommand<TCommand, THandler>(Action<RequestBuilder<TCommand>>? build = null)
            where TCommand : Command
            where THandler : class, ICommandHandler<TCommand>
        {
            var builder = new RequestBuilder<TCommand>(new RequestOptions<TCommand>(), _builder.Services);
            builder.AddPreHandler<ValidationBehavior<TCommand>>();

            build?.Invoke(builder);

            _canonicalTypeRegistry.Add(typeof(TCommand));
            _handlerRegistry.Add(typeof(TCommand), typeof(THandler), builder.Options.PreHandlers);

            _builder.Services.AddTransient<THandler>();

            return this;
        }

        public KernelBuilder AddQuery<TQuery, TResult, THandler>(Action<RequestBuilder<TQuery>>? build = null)
            where TQuery : Query<TResult>
            where THandler : class, IQueryHandler<TQuery, TResult>
        {
            var builder = new RequestBuilder<TQuery>(new RequestOptions<TQuery>(), _builder.Services);
            builder.AddPreHandler<ValidationBehavior<TQuery>>();

            build?.Invoke(builder);

            _canonicalTypeRegistry.Add(typeof(TQuery));
            _handlerRegistry.Add(typeof(TQuery), typeof(THandler), builder.Options.PreHandlers);

            _builder.Services.AddTransient<THandler>();

            return this;
        }

        public KernelBuilder AddCommandSender(Uri url)
        {
            _builder.Services.AddGrpcClient<GenericAPI.GenericAPIClient>(options => options.Address = url);
            _builder.Services.AddTransient<IRequestSender, GrpcRequestSender>();

            return this;
        }

        public KernelBuilder AddGrpc(int port = 8080)
        {
            _builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<ErrorHandlingInterceptor>();
                options.Interceptors.Add<LoggerInterceptor>();
            });
            _builder.Services.AddTransient<IGenericClient, GrpcGenericClient>();

            _builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Loopback, port, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
            });

            return this;
        }
    }
}
