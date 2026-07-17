using Ironyx.Kernel.Builders;
using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Generators;
using Ironyx.Kernel.Registry;
using Ironyx.Kernel.Serializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Ironyx.Kernel
{
    [ExcludeFromCodeCoverage]
    public static class WebApplicationBuilderExtensions
    {
        public static KernelBuilder UseKernel(this WebApplicationBuilder builder)
        {
            builder.Services.AddCommandDispatcher();

            builder.Services.AddGrpc();

            builder.Services.AddTransient<IExtractor, RequestContextExtractor>();

            var canonicalTypeRegistry = new CanonicalTypeRegistry();
            builder.Services.AddSingleton(_ => canonicalTypeRegistry);
            builder.Services.AddTransient<ICanonicalTypeBuilder>(p => p.GetRequiredService<CanonicalTypeRegistry>());
            builder.Services.AddTransient<IRuntimeTypeResolver>(p => p.GetRequiredService<CanonicalTypeRegistry>());

            builder.Services.AddScoped<RequestContext>();
            builder.Services.AddTransient<IRequestContext>(provider => provider.GetRequiredService<RequestContext>());
            builder.Services.AddScoped<IRequestContextAccessor>(provider => provider.GetRequiredService<RequestContext>());

            builder.Services.AddTransient<IUlidGenerator, ULidGenerator>();

            builder.Services.AddTransient<IRequestDeserializer, RequestDeserializer>();

            builder.WebHost.ConfigureKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 57400, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            }));

            return new KernelBuilder(builder, canonicalTypeRegistry);
        }
    }
}
