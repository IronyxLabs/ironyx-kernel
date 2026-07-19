using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Execution.Dispatchers
{
    public class QueryDispatcher : IQueryDispatcher
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<QueryDispatcher> _logger;

        public QueryDispatcher(IServiceProvider provider, ILogger<QueryDispatcher> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task<TResult> DispatchAsync<TResult>(Query<TResult> query, CancellationToken cancellationToken)
        {
            var type = query.GetType();

            _logger.LogDebug("Dispatching {Query}", type.Name);
            var handlerType = typeof(IQueryHandler<,>).MakeGenericType(type, typeof(TResult));
            var handler = _provider.GetService(handlerType) ?? throw new InvalidOperationException($"Handler not found for query: {type.FullName}");

            var result = await ((dynamic)handler).HandleAsync((dynamic)query, cancellationToken);
            _logger.LogDebug("Dispatching {Query} has been finished", type.Name);
            //_logger.LogTrace("Result: {@Result}", result);

            return result;
        }
    }
}
