using Ironyx.Kernel.Receivers;
using Microsoft.AspNetCore.Builder;
using System.Diagnostics.CodeAnalysis;

namespace Ironyx.Kernel
{
    [ExcludeFromCodeCoverage]
    public static class WebApplicationExtensions
    {
        public static void MapKernel(this WebApplication application)
        {
            application.MapGrpcService<GrpcEndpoint>();
        }
    }
}
