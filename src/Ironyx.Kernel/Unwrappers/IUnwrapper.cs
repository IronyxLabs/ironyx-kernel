using Grpc.Core;

namespace Ironyx.Kernel.Unwrappers
{
    public interface IUnwrapper
    {
        Task UnwrapAsync(Metadata metadata, CancellationToken cancellationToken);
    }
}
