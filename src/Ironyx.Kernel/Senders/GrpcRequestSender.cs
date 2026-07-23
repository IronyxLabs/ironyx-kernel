using Grpc.Core;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Execution.Senders;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace Ironyx.Kernel.Senders
{
    public class GrpcRequestSender : IRequestSender
    {
        private readonly IGenericClient _client;
        private readonly IEnricher _enricher;
        private readonly ILogger<GrpcRequestSender> _logger;

        public GrpcRequestSender(IGenericClient client, IEnricher enricher, ILogger<GrpcRequestSender> logger)
        {
            _client = client;
            _enricher = enricher;
            _logger = logger;
        }

        public async Task<TResult?> GetAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken) where TQuery : Query<TResult>
        {
            var type = query.GetType();
            _logger.LogDebug("Sending query: {Query}", type.FullName);

            var envelop = new Envelop()
            {
                Type = type.FullName,
                Version = type.GetVersion(),
                Payload = await query.SerializeAsync(cancellationToken)
            };

            var metadata = new Metadata();
            await _enricher.EnrichAsync(metadata, cancellationToken);

            _logger.LogTrace("Envelop: {@Envelop}", envelop);
            _logger.LogTrace("Metadata: {@Metadata}", metadata);

            var reply = await _client.GetAsync(envelop, metadata, cancellationToken);
            return await reply.Data.DeserializeAsync<TResult>(cancellationToken);
        }

        public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : Command
        {
            var type = command.GetType();
            _logger.LogDebug("Sending command: {Command}", type.FullName);

            var envelop = new Envelop()
            {
                Type = type.FullName,
                Version = type.GetVersion(),
                Payload = await command.SerializeAsync(cancellationToken)
            };
            var metadata = new Metadata();
            await _enricher.EnrichAsync(metadata, cancellationToken);

            _logger.LogTrace("Envelop: {@Envelop}", envelop);
            _logger.LogTrace("Metadata: {@Metadata}", metadata);

            await _client.SendAsync(envelop, metadata, cancellationToken);
        }
    }

    file static class Exceptions
    {
        public static InvalidOperationException VersionNotFound(Type type) => new($"Version not found for {type.FullName} command");
    }

    file static class GrpcRequestSenderExceptions
    {
        public static string GetVersion(this Type type)
        {
            return type.GetCustomAttribute<RequestVersionAttribute>()?.Version ?? throw Exceptions.VersionNotFound(type);
        }

        public static async Task<string> SerializeAsync(this object value, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            await JsonSerializer.SerializeAsync(stream, value, cancellationToken: cancellationToken);
            stream.Position = 0;

            return await reader.ReadToEndAsync(cancellationToken);
        }

        public static async Task<TResult?> DeserializeAsync<TResult>(this string json, CancellationToken cancellationToken)
        {
            await using var stream = new MemoryStream();
            await using var writer = new StreamWriter(stream);

            await writer.WriteAsync(json);
            await writer.FlushAsync(cancellationToken);
            stream.Position = 0;

            return await JsonSerializer.DeserializeAsync<TResult>(stream, cancellationToken: cancellationToken);
        }
    }
}
