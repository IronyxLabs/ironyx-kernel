using Grpc.Core;

namespace Ironyx.Kernel.Enrichers
{
    public interface IEnricher
    {
        Task EnrichAsync(Metadata metadata, CancellationToken cancellationToken);
    }
}
