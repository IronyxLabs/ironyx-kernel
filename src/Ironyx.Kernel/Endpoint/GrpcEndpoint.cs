using Grpc.Core;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Serializers;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Ironyx.Kernel.Receivers
{
    [ExcludeFromCodeCoverage]
    public class GrpcEndpoint : GenericAPI.GenericAPIBase
    {
        private readonly IRequestDeserializer _deserializer;
        private readonly IExtractor _extractor;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly IQueryDispatcher _queryDispatcher;
        private readonly ILogger<GrpcEndpoint> _logger;

        public GrpcEndpoint(IRequestDeserializer deserilaizer, IExtractor extractor, IRequestContextAccessor requestContext, ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher, ILogger<GrpcEndpoint> logger)
        {
            _deserializer = deserilaizer;
            _extractor = extractor;
            _requestContext = requestContext;
            _commandDispatcher = commandDispatcher;
            _queryDispatcher = queryDispatcher;
            _logger = logger;
        }

        public override async Task<Reply> SendAsync(Envelop envelop, ServerCallContext context)
        {
            _logger.LogDebug("Receiving command");
            await _extractor.ExtractAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.BeginScope(_requestContext.CreateLogContext());

            await _commandDispatcher.DispatchAsync(await _deserializer.DeserializeAsync(envelop, context.CancellationToken), context.CancellationToken);

            _logger.LogDebug("Command accepted");
            return GrpcReply.Accepted.Reply;
        }

        public override async Task<Reply> GetAsync(Envelop envelop, ServerCallContext context)
        {
            var query = await _deserializer.DeserializeAsync(envelop, context.CancellationToken);

            await _extractor.ExtractAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.BeginScope(_requestContext.CreateLogContext());

            var result = await _queryDispatcher.DispatchAsync<dynamic>(query, context.CancellationToken);
            return GrpcReply.Ok(JsonSerializer.Serialize(result)).Reply;
        }
    }

    file static class GrpcEndpointExtensions
    {
        public static IDictionary<string, object?> CreateLogContext(this IRequestContextAccessor context)
        {
            return new Dictionary<string, object?>()
            {
                ["CorrelationId"] = context.CorrelationId,
                ["CausationId"] = context.CausationId,
                ["RequestId"] = context.RequestId
            };
        }
    }

    file sealed class GrpcReply
    {
        public Status Status { get; }
        public Reply Reply { get; }

        public static GrpcReply Accepted => new(new Status(StatusCode.OK, "Ok"),
                                                                new Reply() { Status = "ACCEPTED" });
        public static GrpcReply Ok(string data) => new(new Status(StatusCode.OK, "Ok"),
                                                                new Reply() { Status = "OK", Data = data });

        public GrpcReply(Status status, Reply reply)
        {
            Status = status;
            Reply = reply;
        }
    }
}
