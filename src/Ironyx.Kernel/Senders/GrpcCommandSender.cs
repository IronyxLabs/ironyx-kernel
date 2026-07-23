using Ironyx.Kernel.Execution.Senders;
using System.Reflection;
using System.Text.Json;

namespace Ironyx.Kernel.Senders
{
    public class GrpcCommandSender : ICommandSender
    {
        private readonly GenericAPI.GenericAPIClient _client;

        public GrpcCommandSender(GenericAPI.GenericAPIClient client)
        {
            _client = client;
        }

        public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : Command
        {
            await using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            await JsonSerializer.SerializeAsync(stream, command, cancellationToken: cancellationToken);
            stream.Position = 0;

            var envelop = new Envelop()
            {
                Type = command.GetType().FullName,
                Version = command.GetType().GetCustomAttribute<RequestVersionAttribute>().Version,
                Payload = await reader.ReadToEndAsync(cancellationToken)
            };

            await _client.SendAsyncAsync(envelop, cancellationToken: cancellationToken);
        }
    }
}
