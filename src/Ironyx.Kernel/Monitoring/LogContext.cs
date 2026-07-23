using Grpc.Core;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Receivers;
using Ironyx.Kernel.Senders;
using Ironyx.Kernel.Serializers;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Monitoring
{
    public partial class LogContext
    {
        public partial class GrpEndpointLogContext(ILogger<GrpcEndpoint> logger)
        {
            [LoggerMessage(Level = LogLevel.Debug, Message = "Receiving command")]
            public partial void ReceivingCommand();

            [LoggerMessage(Level = LogLevel.Debug, Message = "Command has been accepted")]
            public partial void CommandAccepted();


            [LoggerMessage(Level = LogLevel.Debug, Message = "Receiving query")]
            public partial void ReceivingQuery();
            [LoggerMessage(Level = LogLevel.Debug, Message = "Query has been executed")]
            public partial void QueryExecuted();


            public IDisposable SetLogContext(Ulid correlationId, Ulid? causationId, Ulid requestId)
            {
                return logger.BeginScope(new Dictionary<string, object?>()
                {
                    ["CorrelationId"] = correlationId,
                    ["CausationId"] = causationId,
                    ["RequestId"] = requestId
                })!;
            }
        }

        public partial class RequestContextEnricherLogContext(ILogger<RequestContextEnricher> logger)
        {
            [LoggerMessage(Level = LogLevel.Debug, Message = "Set CorrelationId to {CorrelationId}")]
            public partial void LogCorrelationId(Ulid correlationId);

            [LoggerMessage(Level = LogLevel.Debug, Message = "Set CausationId to {CausationId}")]
            public partial void LogCausationId(Ulid? causationId);
        }

        public partial class RequestContextExtractorLogContext(ILogger<RequestContextExtractor> logger)
        {
            [LoggerMessage(Level = LogLevel.Debug, Message = "Correlation Id has been generated: {CorrelationId}")]
            public partial void LogCorrelationIdGenerated(Ulid correlationId);

            [LoggerMessage(Level = LogLevel.Debug, Message = "Correlation Id has been received: {CorrelationId}")]
            public partial void LogCorrelationIdReceived(Ulid correlationId);

            [LoggerMessage(Level = LogLevel.Debug, Message = "Causation Id has not been defined")]
            public partial void LogCausationIdNotDefined();

            [LoggerMessage(Level = LogLevel.Debug, Message = "Causation Id has been received: {CausationId}")]
            public partial void LogCausationIdReceived(Ulid? causationId);

            [LoggerMessage(Level = LogLevel.Debug, Message = "Request Id has been generated: {RequestId}")]
            public partial void LogRequestIdGenerated(Ulid requestId);
        }

        public partial class RequestDeserializerLogContext(ILogger<RequestDeserializer> logger)
        {
            [LoggerMessage(Level = LogLevel.Debug, Message = "Serializing {Type}.{Version}")]
            public partial void LogSerializing(string type, string version);

            [LoggerMessage(Level = LogLevel.Debug, Message = "Request successfully serialized")]
            public partial void LogSerialized();

            [LoggerMessage(Level = LogLevel.Trace, Message = "Request: {@Request}")]
            public partial void LogRequestObject(object request);
        }

        public partial class GrpcRequestSenderLogContext(ILogger<GrpcRequestSender> logger)
        {
            [LoggerMessage(Level = LogLevel.Debug, Message = "Sending query: {Query}")]
            public partial void LogSendingQuery(string query);

            [LoggerMessage(Level = LogLevel.Debug, Message = "Sending command: {Command}")]
            public partial void LogSendingCommand(string command);

            [LoggerMessage(Level = LogLevel.Trace, Message = "Envelop: {@Envelop}")]
            public partial void LogEnvelop(Envelop envelop);

            [LoggerMessage(Level = LogLevel.Trace, Message = "Metadata: {@Metadata}")]
            public partial void LogMetadata(Metadata metadata);
        }
    }
}
