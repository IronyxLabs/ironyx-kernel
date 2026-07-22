using Grpc.Core;
using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Generators;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Extractors
{
    public class RequestContextExtractor : IExtractor
    {
        private readonly IRequestContext _context;
        private readonly IUlidGenerator _generator;
        private readonly ILogger<RequestContextExtractor> _logger;

        public RequestContextExtractor(IRequestContext context, IUlidGenerator generator, ILogger<RequestContextExtractor> logger)
        {
            _context = context;
            _generator = generator;
            _logger = logger;
        }

        public async Task ExtractAsync(Metadata metadata, CancellationToken cancellationToken)
        {
            var correlationId = metadata.GetCorrelationId();
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                _context.CorrelationId = _generator.Get();
                _logger.LogDebug("Correlation Id has been generated: {CorrelationId}", _context.CorrelationId);
            }
            else
            {
                _context.CorrelationId = Ulid.Parse(correlationId);
                _logger.LogDebug("Correlation Id has been received: {CorrelationId}", _context.CorrelationId);
            }

            var causationId = metadata.GetCausationId();
            if (string.IsNullOrWhiteSpace(causationId))
            {
                _logger.LogDebug("Causation Id has not been defined");
            }
            else
            {
                _context.CausationId = Ulid.Parse(causationId);
                _logger.LogDebug("Causation Id has been received: {CorrelationId}", _context.CausationId);

            }
            _context.RequestId = _generator.Get();
            _logger.LogDebug("Request Id has been generated: {RequestId}", _context.RequestId);
        }
    }

    file static class RequestContextUnwrapperExtensions
    {
        public static string? GetCorrelationId(this Metadata metadata)
        {
            return metadata.SingleOrDefault(m => m.Key.Equals("correlation-id", StringComparison.InvariantCultureIgnoreCase))?.Value;
        }
        public static string? GetCausationId(this Metadata metadata)
        {
            return metadata.SingleOrDefault(m => m.Key.Equals("causation-id", StringComparison.InvariantCultureIgnoreCase))?.Value;
        }
    }
}
