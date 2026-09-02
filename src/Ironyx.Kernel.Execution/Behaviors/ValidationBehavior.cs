using FluentValidation;
using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Monitoring;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            _logger.LogValidating(request.GetType().FullName!);

            var validator = _provider.GetService<IValidator<TRequest>>();
            if (validator is null)
            {
                _logger.LogNoValidator(request.GetType().FullName!);
                return;
            }

            await validator.ValidateAndThrowAsync(request, cancellationToken);

            _logger.LogHasBeenValidated(request.GetType().FullName!);
        }
    }
}
