using Grpc.Core;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Monitoring;
using Ironyx.Kernel.Serializers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ironyx.Kernel.Receivers
{
    [ExcludeFromCodeCoverage]
    public partial class GrpcEndpoint : GenericAPI.GenericAPIBase
    {
        private readonly IRequestDeserializer _deserializer;
        private readonly IExtractor _extractor;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly LogContext.GrpEndpointLogContext _logger;

        public GrpcEndpoint(IRequestDeserializer deserilaizer, IExtractor extractor, IRequestContextAccessor requestContext, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, ILogger<GrpcEndpoint> logger)
        {
            _deserializer = deserilaizer;
            _extractor = extractor;
            _requestContext = requestContext;
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _logger = new LogContext.GrpEndpointLogContext(logger);
        }

        public override async Task<Reply> SendAsync(Envelop envelop, ServerCallContext context)
        {
            _logger.ReceivingCommand();
            await _extractor.ExtractAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.SetLogContext(_requestContext.CorrelationId, _requestContext.CausationId, _requestContext.RequestId);

            await _commandDispatcher.DispatchAsync(await _deserializer.DeserializeAsync(envelop, context.CancellationToken), context.CancellationToken);

            _logger.CommandAccepted();
            return new Reply();
        }

        public override async Task<Reply> GetAsync(Envelop envelop, ServerCallContext context)
        {
            _logger.ReceivingQuery();
            var query = await _deserializer.DeserializeAsync(envelop, context.CancellationToken);

            await _extractor.ExtractAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.SetLogContext(_requestContext.CorrelationId, _requestContext.CausationId, _requestContext.RequestId);

            var result = await _queryDispatcher.DispatchAsync<dynamic>(query, context.CancellationToken);

            _logger.QueryExecuted();
            return new Reply()
            {
                Data = JsonSerializer.Serialize(result)
            };
        }
    }
}
