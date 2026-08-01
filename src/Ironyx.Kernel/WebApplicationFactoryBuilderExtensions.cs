using Ironyx.Kernel.Builders;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Execution.Registries;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Generators;
using Ironyx.Kernel.Registry;
using Ironyx.Kernel.Serializers;
using Microsoft.AspNetCore.Builder;
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
            builder.Services.AddQueryDispatcher();

            builder.Services.AddTransient<IEnricher, RequestContextEnricher>();

            builder.Services.AddTransient<IExtractor, RequestContextExtractor>();

            var canonicalTypeRegistry = new CanonicalTypeRegistry();
            builder.Services.AddSingleton(_ => canonicalTypeRegistry);
            builder.Services.AddTransient<ICanonicalTypeBuilder>(p => p.GetRequiredService<CanonicalTypeRegistry>());
            builder.Services.AddTransient<IRuntimeTypeResolver>(p => p.GetRequiredService<CanonicalTypeRegistry>());

            var handlerRegistry = new HandlerRegistry();
            builder.Services.AddSingleton(_ => handlerRegistry);
            builder.Services.AddTransient<IHandlerRegistry>(p => p.GetRequiredService<HandlerRegistry>());
            builder.Services.AddTransient<IHandlerTypeResolver>(p => p.GetRequiredService<HandlerRegistry>());

            builder.Services.AddScoped<RequestContext>();
            builder.Services.AddTransient<IRequestContext>(provider => provider.GetRequiredService<RequestContext>());
            builder.Services.AddTransient<IRequestContextAccessor>(provider => provider.GetRequiredService<RequestContext>());

            builder.Services.AddTransient<IUlidGenerator, ULidGenerator>();

            builder.Services.AddTransient<IRequestDeserializer, RequestDeserializer>();

            return new KernelBuilder(builder, canonicalTypeRegistry, handlerRegistry);
        }
    }
}
