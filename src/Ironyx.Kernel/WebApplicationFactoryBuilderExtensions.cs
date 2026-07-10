using Ironyx.Kernel.Builders;
using Ironyx.Kernel.Unwrappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel
{
    public static class WebApplicationBuilderExtensions
    {
        public static KernelBuilder UseKernel(this WebApplicationBuilder builder)
        {
            builder.Services.AddCommandDispatcher();

            builder.Services.AddGrpc();
            builder.Services.AddTransient<IUnwrapper, RequestUnwrapper>();

            builder.WebHost.ConfigureKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 57400, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            }));

            return new KernelBuilder(builder);
        }
    }
}
