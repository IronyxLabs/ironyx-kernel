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

        public async Task<Reply> GetAsync(Envelop envelop, Metadata metadata, CancellationToken cancellationToken)
        {
            return await _client.GetAsyncAsync(envelop, metadata, cancellationToken: cancellationToken);
        }

        public async Task SendAsync(Envelop envelop, Metadata metadata, CancellationToken cancellationToken)
        {
            await _client.SendAsyncAsync(envelop, metadata, cancellationToken: cancellationToken);
        }
    }
}
