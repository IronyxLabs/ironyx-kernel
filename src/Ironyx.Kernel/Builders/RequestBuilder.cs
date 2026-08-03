using FluentValidation;
using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Behaviors;
using Ironyx.Kernel.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Ironyx.Kernel.Builders
{
    public class RequestBuilder<TRequest>
        where TRequest : IRequest
    {
        private readonly IServiceCollection _services;

        public RequestOptions<TRequest> Options { get; }

        public RequestBuilder(RequestOptions<TRequest> options, IServiceCollection services)
        {
            Options = options;
            _services = services;
        }

        public RequestBuilder<TRequest> AddValidator<TValidator>()
            where TValidator : class, IValidator<TRequest>
        {
            _services.AddTransient<IValidator<TRequest>, TValidator>();

            return this;
        }

        public RequestBuilder<TRequest> AddPreHandler<TPreHandler>()
            where TPreHandler : class, IPreHandler<TRequest>
        {
            _services.AddTransient<TPreHandler>();
            Options.PreHandlers.Add(typeof(TPreHandler));

            return this;
        }
    }
}
