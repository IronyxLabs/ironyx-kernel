using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ironyx.Kernel.Serializers
{
    public class RequestDeserializer : IRequestDeserializer
    {
        private readonly ILogger<RequestDeserializer> _logger;

        public RequestDeserializer(ILogger<RequestDeserializer> logger)
        {
            _logger = logger;
        }

        public async Task<dynamic> DeserializeAsync(Request request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Attempting to serialize incoming request");
            if (string.IsNullOrWhiteSpace(request.Type)) throw Exceptions.TypeIsNotDefined;
            _logger.LogDebug("Detected request type: {RequestType}", request.Type);

            var type = Type.GetType(request.Type) ?? throw Exceptions.UnknowType;

            var command = await request.DeserializeAsync(type, cancellationToken);
            _logger.LogDebug("Command successfully serialized");
            _logger.LogTrace("Command: {@Command}", (object)command);
            return command;

        }
    }

    file static class RequestUnwrapperExtensions
    {
        public static string? GetRequestType(this Metadata metadata)
        {
            return metadata.SingleOrDefault(m => m.Key == "request-type")?.Value;
        }
        public static async Task<dynamic> DeserializeAsync(this Request request, Type type, CancellationToken cancellationToken)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                {
                    await writer.WriteAsync(request.Content.AsMemory(), cancellationToken);
                    await writer.FlushAsync(cancellationToken);
                    stream.Position = 0;

                    return await JsonSerializer.DeserializeAsync(stream, type, cancellationToken: cancellationToken);
                }
            }
        }
    }

    file static class Exceptions
    {
        public static ArgumentNullException TypeIsNotDefined => new("request-type", "The 'request-type' header is not defined");
        public static NotSupportedException UnknowType => new("Unkown request type");
    }
}
