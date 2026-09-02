
using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Extensions;
using Ironyx.Kernel.Execution.Monitoring;
using Ironyx.Kernel.Execution.Registries;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly IHandlerTypeResolver _resolver;
        private readonly CommandDispatcherLogContext _logger;

        public CommandDispatcher(IServiceProvider provider, IHandlerTypeResolver resolver, ILogger<CommandDispatcher> logger)
        {
            _provider = provider;
            _resolver = resolver;
            _logger = new CommandDispatcherLogContext(logger);
        }

        public async Task DispatchAsync(IRequest command, CancellationToken cancellationToken)
        {
            var type = command.GetType();

            _logger.LogDispatchingCommand(type.Name);
            var description = _resolver[type];
            var handler = (dynamic)(_provider.GetService(description.Handler) ?? throw new InvalidOperationException($"Handler not found for command: {type.FullName}"));

            _logger.LogPreHandlersFound(description.PreHandlers.Count());
            await description.PreHandlers.InvokeAsync(command, _provider, cancellationToken);

            await handler.HandleAsync((dynamic)command, cancellationToken);
            _logger.LogDispatchingCommandFinished(type.Name);
        }
    }
}
