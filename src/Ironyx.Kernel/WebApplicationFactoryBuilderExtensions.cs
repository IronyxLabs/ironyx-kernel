using Ironyx.Kernel.Builders;
using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Generators;
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

            builder.Services.AddScoped<RequestContext>();
            builder.Services.AddTransient<IRequestContext>(provider => provider.GetRequiredService<RequestContext>());
            builder.Services.AddScoped<IRequestContextAccessor>(provider => provider.GetRequiredService<RequestContext>());

            builder.Services.AddTransient<IUlidGenerator, ULidGenerator>();

            builder.Services.AddTransient<IRequestDeserializer, RequestDeserializer>();

            builder.WebHost.ConfigureKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 57400, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            }));

            return new KernelBuilder(builder);
        }
    }
}
