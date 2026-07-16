
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
            var type = command.GetType();

            _logger.LogDebug("Dispatching {Command}", type.Name);
            var handler = _provider.GetService<ICommandHandler<TCommand>>() ?? throw new InvalidOperationException($"Handler not found for command: {type.FullName}");

            await handler.HandleAsync(command, cancellationToken);
            _logger.LogDebug("Dispatching {Command} has been finished", type.Name);
        }
    }
}
