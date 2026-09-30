using Grpc.Core;
using Ironyx.Kernel.Execution.Senders;
using Ironyx.Kernel.Handlers;
using Ironyx.Kernel.Interceptors;
using Ironyx.Kernel.Senders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel.Builders
{
    public class GrpcBuilder
    {
        private readonly WebApplicationBuilder _builder;

        public GrpcBuilder(WebApplicationBuilder builder)
        {
            _builder = builder;
        }

        public GrpcBuilder AddRequestSender(Uri url)
        {
            _builder.Services.AddGrpcClient<GenericAPI.GenericAPIClient>(options => options.Address = url);
            _builder.Services.AddTransient<IRequestSender, GrpcRequestSender>();

            _builder.Services.AddTransient<IErrorHandler<RpcException>, GrpcErrorHandler>();

            return this;
        }

        public GrpcBuilder AddRequestReceiver(int port)
        {
            _builder.Services.AddGrpc(options =>
            {
                options.Interceptors.Add<ErrorHandlingInterceptor>();
                options.Interceptors.Add<LoggerInterceptor>();
            });


            _builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(System.Net.IPAddress.Loopback, port, listenOptions => listenOptions.Protocols = HttpProtocols.Http2);
            });

            return this;
        }
    }
}
