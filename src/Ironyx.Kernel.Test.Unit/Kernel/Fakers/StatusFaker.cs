using AutoBogus;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Status = Google.Rpc.Status;

namespace Ironyx.Kernel.Test.Unit.Kernel.Fakers
{
    public class StatusFaker : AutoFaker<Status>
    {
        public StatusFaker InternalServerError()
        {
            RuleFor(s => s.Code, (int)StatusCode.Internal);
            FinishWith((f, s) =>
            {
                var errorInfo = new ErrorInfo
                {
                    Domain = f.Company.CompanyName(),
                    Reason = "INTERNAL_SERVER_ERROR"
                };
                errorInfo.Metadata.Add("Ironyx.ErrorInfo.CorrelationId", f.Random.Guid().ToString());
                s.Details.Add(Any.Pack(errorInfo));
            });

            return this;
        }

        public StatusFaker NotFound()
        {
            RuleFor(s => s.Code, (int)StatusCode.NotFound);

            return this;
        }
    }
}
