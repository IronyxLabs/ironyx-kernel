using Grpc.Core;
using Ironyx.Kernel.Monitoring;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Enrichers
{
    public class RequestContextEnricher : IEnricher
    {
        private readonly IRequestContextAccessor _requestContext;
        private readonly LogContext.RequestContextEnricherLogContext _logger;

        public RequestContextEnricher(IRequestContextAccessor requestContext, ILogger<RequestContextEnricher> logger)
        {
            _requestContext = requestContext;
            _logger = new LogContext.RequestContextEnricherLogContext(logger);
        }

        public Task EnrichAsync(Metadata metadata, CancellationToken cancellationToken)
        {
            _logger.LogCorrelationId(_requestContext.CorrelationId);
            metadata.Add("correlation-id", _requestContext.CorrelationId.ToString());

            _logger.LogCausationId(_requestContext.CausationId);
            metadata.Add("causation-id", _requestContext.RequestId.ToString());

            return Task.CompletedTask;
        }
    }
}
