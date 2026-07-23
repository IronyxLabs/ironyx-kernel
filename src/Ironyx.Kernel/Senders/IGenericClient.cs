using Grpc.Core;

namespace Ironyx.Kernel.Senders
{
    public interface IGenericClient
    {
        Task SendAsync(Envelop envelop, Metadata metadata, CancellationToken cancellationToken);
        Task<Reply> GetAsync(Envelop envelop, Metadata metadata, CancellationToken cancellationToken);
    }
}
