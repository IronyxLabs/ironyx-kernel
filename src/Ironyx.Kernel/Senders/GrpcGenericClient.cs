using Grpc.Core;

namespace Ironyx.Kernel.Senders
{
    public class GrpcGenericClient : IGenericClient
    {
        private readonly GenericAPI.GenericAPIClient _client;

        public GrpcGenericClient(GenericAPI.GenericAPIClient client)
        {
            _client = client;
        }

        public async Task SendAsync(Envelop envelop, Metadata metadata, CancellationToken cancellationToken)
        {
            await _client.SendAsyncAsync(envelop, metadata, cancellationToken: cancellationToken);
        }
    }
}
