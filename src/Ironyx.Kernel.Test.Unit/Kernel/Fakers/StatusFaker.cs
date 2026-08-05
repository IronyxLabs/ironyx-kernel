using AutoBogus;
using Bogus;
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
            FinishWith((f, s) => s.AddErrorInfo("INTERNAL_SERVER_ERROR", f));

            return this;
        }

        public StatusFaker NotFound()
        {
            RuleFor(s => s.Code, (int)StatusCode.NotFound);
            FinishWith((f, s) => s.AddErrorInfo("RESOURCE_NOT_FOUND", f)
                                    .AddResourceInfo());

            return this;
        }

        private void ErrorInfo(Faker faker, Status status, string reason)
        {

        }
    }

    file static class StatusFakerExtensions
    {
        public static Status AddResourceInfo(this Status status)
        {
            status.Details.Add(Any.Pack(new AutoFaker<ResourceInfo>().Generate()));

            return status;
        }

        public static Status AddErrorInfo(this Status status, string reason, Faker faker)
        {
            var errorInfo = new ErrorInfo
            {
                Domain = faker.Company.CompanyName(),
                Reason = reason
            };
            errorInfo.Metadata.Add("Ironyx.ErrorInfo.CorrelationId", faker.Random.Guid().ToString());
            status.Details.Add(Any.Pack(errorInfo));

            return status;
        }
    }
}
