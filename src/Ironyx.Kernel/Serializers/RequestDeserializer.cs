using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Ironyx.Kernel.Serializers
{
    public class RequestDeserializer : IRequestDeserializer
    {
        private readonly ILogger<RequestDeserializer> _logger;

        public RequestDeserializer(ILogger<RequestDeserializer> logger)
        {
            _logger = logger;
        }

        public async Task<dynamic> DeserializeAsync(Envelop envelop, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Attempting to serialize incoming request");
            if (string.IsNullOrWhiteSpace(envelop.Type)) throw Exceptions.TypeIsNotDefined;
            _logger.LogDebug("Detected request type: {RequestType}", envelop.Type);

            var type = Type.GetType(envelop.Type) ?? throw Exceptions.UnknowType;

            //var command = await envelop.DeserializeAsync(type, cancellationToken);
            //_logger.LogDebug("Command successfully serialized");
            //_logger.LogTrace("Command: {@Command}", (object)command);
            //return command;

            return null;
        }
    }

    file static class RequestUnwrapperExtensions
    {
        public static string? GetRequestType(this Metadata metadata)
        {
            return metadata.SingleOrDefault(m => m.Key == "request-type")?.Value;
        }
        //public static async Task<dynamic> DeserializeAsync(this Request request, Type type, CancellationToken cancellationToken)
        //{
        //    using (var stream = new MemoryStream())
        //    {
        //        using (var writer = new StreamWriter(stream, leaveOpen: true))
        //        {
        //            await writer.WriteAsync(request.Content.AsMemory(), cancellationToken);
        //            await writer.FlushAsync(cancellationToken);
        //            stream.Position = 0;

        //            return await JsonSerializer.DeserializeAsync(stream, type, cancellationToken: cancellationToken) ?? throw new JsonException("Invalid content");
        //        }
        //    }
        //}
    }

    file static class Exceptions
    {
        public static ArgumentNullException TypeIsNotDefined => new("Type", "The request type is not defined");
        public static NotSupportedException UnknowType => new("Unkown request type");
    }
}
