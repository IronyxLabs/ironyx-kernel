using Grpc.Core;

namespace Ironyx.Kernel.Unwrappers
{
    public interface IUnwrapper
    {
        Task<dynamic> UnwrapAsync(Request request, Metadata metadata, CancellationToken cancellationToken);
    }
}
