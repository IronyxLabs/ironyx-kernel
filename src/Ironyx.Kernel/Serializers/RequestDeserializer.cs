using Ironyx.Kernel.Monitoring;
using Ironyx.Kernel.Registry;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ironyx.Kernel.Serializers
{
    public class RequestDeserializer : IRequestDeserializer
    {
        private readonly IRuntimeTypeResolver _typeResolver;
        private readonly LogContext.RequestDeserializerLogContext _logger;

        public RequestDeserializer(IRuntimeTypeResolver typeResolver, ILogger<RequestDeserializer> logger)
        {
            _typeResolver = typeResolver;
            _logger = new LogContext.RequestDeserializerLogContext(logger);
        }

        public async Task<dynamic> DeserializeAsync(Envelop envelop, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(envelop.Type)) throw Exceptions.TypeIsNotDefined;
            if (string.IsNullOrWhiteSpace(envelop.Version)) throw Exceptions.VersionIsNotDefined;

            _logger.LogSerializing(envelop.Type, envelop.Version);
            var type = _typeResolver[envelop.Type, envelop.Version];

            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream, leaveOpen: true);
            await writer.WriteAsync(envelop.Payload.AsMemory(), cancellationToken);
            await writer.FlushAsync(cancellationToken);
            stream.Position = 0;

            var result = await JsonSerializer.DeserializeAsync(stream, type, cancellationToken: cancellationToken) ?? throw new JsonException("Invalid content");
            _logger.LogSerialized();
            _logger.LogRequestObject(result);

            return result;
        }
    }

    file static class Exceptions
    {
        public static ArgumentNullException TypeIsNotDefined => new("Type", "The request type is not defined");
        public static ArgumentNullException VersionIsNotDefined => new("Version", "The request version is not defined");
    }
}
