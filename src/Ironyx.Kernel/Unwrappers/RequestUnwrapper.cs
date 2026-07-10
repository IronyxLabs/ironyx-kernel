using Grpc.Core;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ironyx.Kernel.Unwrappers
{
    public class RequestUnwrapper : IUnwrapper
    {
        private readonly ILogger<RequestUnwrapper> _logger;

        public RequestUnwrapper(ILogger<RequestUnwrapper> logger)
        {
            _logger = logger;
        }

        public async Task<dynamic> UnwrapAsync(Request request, Metadata metadata, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Attempting to serialize incoming request");
            var typeValue = metadata.GetRequestType() ?? throw Exceptions.TypeIsNotDefined;
            _logger.LogDebug("Detected request type: {request-type}", typeValue);

            var type = Type.GetType(typeValue) ?? throw Exceptions.UnknowType;

            var command = await request.DeserializeAsync(type, cancellationToken);
            _logger.LogDebug("Command succesfully serialized");
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
                    await writer.WriteAsync(request.Body.AsMemory(), cancellationToken);
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
