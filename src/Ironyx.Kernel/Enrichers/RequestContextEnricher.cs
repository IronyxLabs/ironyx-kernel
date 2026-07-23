using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Enrichers
{
    public class RequestContextEnricher : IEnricher
    {
        private readonly IRequestContextAccessor _requestContext;
        private readonly ILogger<RequestContextEnricher> _logger;

        public RequestContextEnricher(IRequestContextAccessor requestContext, ILogger<RequestContextEnricher> logger)
        {
            _requestContext = requestContext;
            _logger = logger;
        }

        public Task EnrichAsync(Metadata metadata, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Set CorrelationId to {CorrelationId}", _requestContext.CorrelationId);
            metadata.Add("correlation-id", _requestContext.CorrelationId.ToString());

            _logger.LogDebug("Set CorrelationId to {CorrelationId}", _requestContext.RequestId);
            metadata.Add("causation-id", _requestContext.RequestId.ToString());

            return Task.CompletedTask;
        }
    }
}
