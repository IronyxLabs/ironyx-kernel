using Ironyx.Testing;

namespace Ironyx.Kernel.Test.Features
{
    public class GrpcEndpointFeatureAttribute : FeatureAttribute
    {
        public GrpcEndpointFeatureAttribute() : base("GRE", "gRPC Endpoint") { }
    }

    public class CommandHandlingFeatureAttribute : FeatureAttribute
    {
        public CommandHandlingFeatureAttribute() : base("CMD", "Command Handling") { }
    }

    public class QueryHandlingFeatureAttribute : FeatureAttribute
    {
        public QueryHandlingFeatureAttribute() : base("QRY", "Query Handling") { }
    }
}
