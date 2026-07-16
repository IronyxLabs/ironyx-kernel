using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel.Builders
{
    public class KernelBuilder
    {
        private readonly WebApplicationBuilder _builder;

        public KernelBuilder(WebApplicationBuilder builder)
        {
            _builder = builder;
        }

        public KernelBuilder AddHandler<TCommand, TCommandHandler>()
            where TCommand : Command
            where TCommandHandler : class, ICommandHandler<TCommand>
        {
            _builder.Services.AddTransient<ICommandHandler<TCommand>, TCommandHandler>();

            return this;
        }
    }
}
