using Ironyx.Testing;

namespace Ironyx.Kernel.Test.Unit
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

    public class RequestSendingFeatureAttribute : FeatureAttribute
    {
        public RequestSendingFeatureAttribute() : base("RQS", "Request Sending") { }
    }
}
