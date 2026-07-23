using Grpc.Core;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Execution.Senders;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace Ironyx.Kernel.Senders
{
    public class GrpcCommandSender : ICommandSender
    {
        private readonly IGenericClient _client;
        private readonly IEnricher _enricher;
        private readonly ILogger<GrpcCommandSender> _logger;

        public GrpcCommandSender(IGenericClient client, IEnricher enricher, ILogger<GrpcCommandSender> logger)
        {
            _client = client;
            _enricher = enricher;
            _logger = logger;
        }

        public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : Command
        {
            var type = command.GetType();
            _logger.LogDebug("Sending command: {Command}", type.FullName);

            var attribute = type.GetCustomAttribute<RequestVersionAttribute>() ?? throw new InvalidOperationException($"Version not found for {type.FullName} command");

            await using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            await JsonSerializer.SerializeAsync(stream, command, cancellationToken: cancellationToken);
            stream.Position = 0;

            var envelop = new Envelop()
            {
                Type = type.FullName,
                Version = attribute.Version,
                Payload = await reader.ReadToEndAsync(cancellationToken)
            };
            _logger.LogTrace("Envelop: {@Envelop}", envelop);

            var metadata = new Metadata();
            await _enricher.EnrichAsync(metadata, cancellationToken);

            await _client.SendAsync(envelop, metadata, cancellationToken);
        }
    }
}
