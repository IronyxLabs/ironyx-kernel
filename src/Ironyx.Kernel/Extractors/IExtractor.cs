using Grpc.Core;

namespace Ironyx.Kernel.Extractors
{
    public interface IExtractor
    {
        Task ExtractAsync(Metadata metadata, CancellationToken cancellationToken);
    }
}
