
using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Registries;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly IHandlerTypeResolver _resolver;
        private readonly ILogger<CommandDispatcher> _logger;

        public CommandDispatcher(IServiceProvider provider, IHandlerTypeResolver resolver, ILogger<CommandDispatcher> logger)
        {
            _provider = provider;
            _resolver = resolver;
            _logger = logger;
        }

        public async Task DispatchAsync(IRequest command, CancellationToken cancellationToken)
        {
            var type = command.GetType();

            _logger.LogDebug("Dispatching {Command}", type.Name);
            var description = _resolver[type];
            var handler = (dynamic)(_provider.GetService(description.Handler) ?? throw new InvalidOperationException($"Handler not found for command: {type.FullName}"));

            foreach (var preHandlerType in description.PreHandlers)
            {
                var preHandler = (dynamic)_provider.GetService(preHandlerType)!;
                await preHandler.HandleAsync((dynamic)command, cancellationToken).ConfigureAwait(false);
            }

            await handler.HandleAsync((dynamic)command, cancellationToken);
            _logger.LogDebug("Dispatching {Command} has been finished", type.Name);
        }
    }
}
