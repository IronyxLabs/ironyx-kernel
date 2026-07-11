using Ironyx.Kernel.Receivers;
using Microsoft.AspNetCore.Builder;

namespace Ironyx.Kernel
{
    public static class WebApplicationExtensions
    {
        public static void MapKernel(this WebApplication application)
        {
            application.MapGrpcService<GrpcEndpoint>();
        }
    }
}
