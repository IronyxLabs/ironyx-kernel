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

            try
            {
                await _commandDispatcher.DispatchAsync(await _deserializer.DeserializeAsync(envelop, context.CancellationToken), context.CancellationToken);

                _logger.LogDebug("Command accepted");
                return GrpcReply.Accepted.Reply;
            }
            catch (ArgumentNullException exception) when (exception.ParamName == nameof(envelop.Type))
            {
                _logger.LogError(exception, "Error during receive command");
                context.Status = GrpcReply.TypeIsNotDefined.Status;
                return GrpcReply.TypeIsNotDefined.Reply;
            }
            catch (ArgumentNullException exception) when (exception.ParamName == nameof(envelop.Version))
            {
                _logger.LogError(exception, "Error during receive command");
                context.Status = GrpcReply.VersionIsNotDefined.Status;
                return GrpcReply.TypeIsNotDefined.Reply;
            }
            catch (NotSupportedException exception)
            {
                _logger.LogError(exception, "Error during receive command");
                context.Status = GrpcReply.UnknownRequestType.Status;
                return GrpcReply.UnknownRequestType.Reply;
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Error during receive command");
                context.Status = GrpcReply.InvalidBody.Status;
                return GrpcReply.InvalidBody.Reply;
            }
        }

        public override async Task<Reply> GetAsync(Envelop envelop, ServerCallContext context)
        {
            var query = await _deserializer.DeserializeAsync(envelop, context.CancellationToken);
            var result = await _queryDispatcher.DispatchAsync(query, context.CancellationToken);
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
        public static GrpcReply TypeIsNotDefined => new(new Status(StatusCode.InvalidArgument, "Request type is not defined"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_REQUEST_TYPE_IS_MISSING", Message = "Request Type is not defined" } });
        public static GrpcReply VersionIsNotDefined => new(new Status(StatusCode.InvalidArgument, "Version is not defined"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_VERSION_IS_MISSING", Message = "Version is not defined" } });
        public static GrpcReply UnknownRequestType => new(new Status(StatusCode.InvalidArgument, "Unknow request type"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_UNKNOWN_REQUEST_TYPE", Message = "Unknow request type" } });
        public static GrpcReply InvalidBody => new(new Status(StatusCode.InvalidArgument, "Invalid request body"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_INVALID_REQUEST_BODY", Message = "Invalid request body" } });
        public static GrpcReply Ok(string data) => new(new Status(StatusCode.OK, "Ok"),
                                                                new Reply() { Status = "OK", Data = data });

        public GrpcReply(Status status, Reply reply)
        {
            Status = status;
            Reply = reply;
        }
    }
}
