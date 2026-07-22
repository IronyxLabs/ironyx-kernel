using Ironyx.Kernel.Abstraction.Interfaces;
using Ironyx.Kernel.Execution.Registries;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly IHandlerTypeResolver _resolver;
        private readonly ILogger<QueryDispatcher> _logger;

        public QueryDispatcher(IServiceProvider provider, IHandlerTypeResolver resolver, ILogger<QueryDispatcher> logger)
        {
            _provider = provider;
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<TResult> DispatchAsync<TResult>(IQuery query, CancellationToken cancellationToken)
        {
            var type = query.GetType();

            _logger.LogDebug("Dispatching {Query}", type.Name);
            var handler = (dynamic)(_provider.GetService(_resolver[type]) ?? throw new InvalidOperationException($"Handler not found for query: {type.FullName}"));

            var result = await handler.HandleAsync((dynamic)query, cancellationToken);
            _logger.LogDebug("Dispatching {Query} has been finished", type.Name);

            return result;
        }
    }
}
