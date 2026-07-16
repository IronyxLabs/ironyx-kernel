using Grpc.Core;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Serializers;
using Ironyx.Kernel.Unwrappers;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ironyx.Kernel.Receivers
{
    public class GrpcEndpoint : GenericAPI.GenericAPIBase
    {
        private readonly IRequestDeserializer _deserializer;
        private readonly IUnwrapper _unwrapper;
        private readonly IRequestContextAccessor _requestContext;
        private readonly ICommandDispatcher _commandDispatcher;
        private readonly ILogger<GrpcEndpoint> _logger;

        public GrpcEndpoint(IRequestDeserializer deserilaizer, IUnwrapper unwrapper, IRequestContextAccessor requestContext, ICommandDispatcher commandDispatcher, ILogger<GrpcEndpoint> logger)
        {
            _deserializer = deserilaizer;
            _unwrapper = unwrapper;
            _requestContext = requestContext;
            _commandDispatcher = commandDispatcher;
            _logger = logger;
        }

        public override async Task<Reply> SendAsync(Request request, ServerCallContext context)
        {
            _logger.LogDebug("Receiving command");
            await _unwrapper.UnwrapAsync(context.RequestHeaders, context.CancellationToken);
            using var scope = _logger.BeginScope(_requestContext.CreateLogContext());

            try
            {
                await _commandDispatcher.DispatchAsync(await _deserializer.DeserializeAsync(request, context.CancellationToken), context.CancellationToken);

                _logger.LogDebug("Command accepted");
                return GrpcReply.Accepted.Reply;
            }
            catch (ArgumentNullException exception)
            {
                _logger.LogError(exception, "Error during receive command");
                context.Status = GrpcReply.TypeIsNotDefined.Status;
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

    file class GrpcReply
    {
        public Status Status { get; }
        public Reply Reply { get; }

        public static GrpcReply Accepted => new(new Status(StatusCode.OK, "Ok"),
                                                                new Reply() { Status = "ACCEPTED" });
        public static GrpcReply TypeIsNotDefined => new(new Status(StatusCode.InvalidArgument, "The 'request-type' header is not defined"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_REQUEST_TYPE_IS_MISSING", Message = "The 'request-type' header is not defined" } });
        public static GrpcReply UnknownRequestType => new(new Status(StatusCode.InvalidArgument, "Unknow request type"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_UNKNOWN_REQUEST_TYPE", Message = "Unknow request type" } });
        public static GrpcReply InvalidBody => new(new Status(StatusCode.InvalidArgument, "Invalid request body"),
                                                       new Reply() { Status = "ERROR", Error = new Error { Code = "TECH_INVALID_REQUEST_BODY", Message = "Invalid request body" } });

        public GrpcReply(Status status, Reply reply)
        {
            Status = status;
            Reply = reply;
        }
    }
}
