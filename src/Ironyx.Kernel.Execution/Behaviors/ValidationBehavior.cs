using FluentValidation;
using Ironyx.Kernel.Abstraction.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Ironyx.Kernel.Execution.Monitoring.LogContext;

namespace Ironyx.Kernel.Execution.Behaviors
{
    public class ValidationBehavior<TRequest> : IPreHandler<TRequest>
        where TRequest : IRequest
    {
        private readonly IServiceProvider _provider;
        private readonly ValidationBehaviorLogContext<TRequest> _logger;

        public ValidationBehavior(IServiceProvider provider, ILogger<ValidationBehavior<TRequest>> logger)
        {
            _provider = provider;
            _logger = new ValidationBehaviorLogContext<TRequest>(logger);
        }

        public async Task HandleAsync(TRequest request, CancellationToken cancellationToken = default)
        {
            var validator = _provider.GetService<IValidator<TRequest>>();
            await validator.ValidateAndThrowAsync(request, cancellationToken);
        }
    }
}
