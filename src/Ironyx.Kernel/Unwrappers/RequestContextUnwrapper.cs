using Grpc.Core;
using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Generators;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Unwrappers
{
    public class RequestContextUnwrapper : IUnwrapper
    {
        private readonly IUnwrapper _unwrapper;
        private readonly IRequestContext _context;
        private readonly IUlidGenerator _generator;
        private readonly ILogger<RequestContextUnwrapper> _logger;

        public RequestContextUnwrapper(IUnwrapper unwrapper, IRequestContext context, IUlidGenerator generator, ILogger<RequestContextUnwrapper> logger)
        {
            _unwrapper = unwrapper;
            _context = context;
            _generator = generator;
            _logger = logger;
        }

        public async Task<dynamic> UnwrapAsync(Request request, Metadata metadata, CancellationToken cancellationToken)
        {
            _context.RequestId = _generator.Get();
            _logger.LogDebug("Request Id has been generated: {RequestId}", _context.RequestId);

            _logger.LogDebug("Attempting to get Causation Id");
            _context.CausationId = metadata.GetCausationId();
            if (_context.CausationId is null) _logger.LogDebug("No Causation Id has been found");
            else _logger.LogDebug("Causation Id has been found: {CausationId}", _context.CausationId);

            _logger.LogDebug("Attempting to get Correlation Id");
            var correlationId = metadata.GetCorrelationId();
            if (correlationId is null)
            {
                _context.CorrelationId = _generator.Get();
                _logger.LogDebug("Correlation Id has been generated: {CorrelationId}", _context.CorrelationId);
            }
            else
            {
                _context.CorrelationId = correlationId.Value;
                _logger.LogDebug("Correlation Id has been found: {CorrelationId}", _context.CausationId);
            }

            return await _unwrapper.UnwrapAsync(request, metadata, cancellationToken);
        }
    }

    file static class RequestContextUnwrapperExtensions
    {
        public static Ulid? GetCausationId(this Metadata metadata)
        {
            var header = metadata.SingleOrDefault(m => m.Key == "causation-id");
            if (string.IsNullOrWhiteSpace(header?.Value)) return null;

            return Ulid.Parse(header.Value);
        }
        public static Ulid? GetCorrelationId(this Metadata metadata)
        {
            var header = metadata.SingleOrDefault(m => m.Key == "correlation-id");
            if (string.IsNullOrWhiteSpace(header?.Value)) return null;

            return Ulid.Parse(header.Value);
        }
    }
}
