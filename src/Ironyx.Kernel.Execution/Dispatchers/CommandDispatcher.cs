
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public class CommandDispatcher : ICommandDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<CommandDispatcher> _logger;

        public CommandDispatcher(IServiceProvider provider, ILogger<CommandDispatcher> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : Command
        {
            _logger.LogDebug("Dispatching {command}", command.GetType().Name);
            var handler = _provider.GetService<ICommandHandler<TCommand>>() ?? throw new InvalidOperationException($"Handler not found for command: {command.GetType().FullName}");

            await handler.HandleAsync(command, cancellationToken);
            _logger.LogDebug("Dispatching {command} has been finished", command.GetType().Name);
        }
    }
}
