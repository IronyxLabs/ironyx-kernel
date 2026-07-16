
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public class LogDispatcher : ICommandDispatcher
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ILogger<LogDispatcher> _logger;

        public LogDispatcher(ICommandDispatcher dispatcher, IRequestContextAccessor requestContext, ILogger<LogDispatcher> logger)
        {
            _dispatcher = dispatcher;
            _requestContext = requestContext;
            _logger = logger;
        }

        public async Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken) where TCommand : Command
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object?>()
            {
                ["CorrelationId"] = _requestContext.CorrelationId,
                ["CausationId"] = _requestContext.CausationId,
                ["RequestId"] = _requestContext.RequestId,
            });

            await _dispatcher.DispatchAsync(command, cancellationToken).ConfigureAwait(false);
        }
    }
}
