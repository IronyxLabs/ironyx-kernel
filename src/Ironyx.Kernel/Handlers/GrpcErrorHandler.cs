using Google.Rpc;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static Ironyx.Kernel.Monitoring.LogContext;
using Status = Google.Rpc.Status;

namespace Ironyx.Kernel.Handlers
{
    public class GrpcErrorHandler : IErrorHandler<RpcException>
    {
        private readonly GrpcErrorHandlerLogContext _logger;

        public GrpcErrorHandler(ILogger<GrpcErrorHandler> logger)
        {
            _logger = new GrpcErrorHandlerLogContext(logger);
        }

        public void Handle(RpcException exception)
        {
            var status = exception.GetRpcStatus();
            switch (status!.Code)
            {
                case (int)StatusCode.NotFound:
                    throw new NotFoundException(status.Message, exception);
                case (int)StatusCode.Internal:
                    throw status.AsInternalServerError();
                default:
                    break;
            }
        }
    }

    file static class GrpcErrorHandlerExtensions
    {
        public static InvalidOperationException AsInternalServerError(this Status status)
        {
            var result = new InvalidOperationException("An internal server error occured");
            result.ExtractErrorInfo(status.GetDetail<ErrorInfo>());

            return result;
        }

        public static void ExtractErrorInfo(this Exception exception, ErrorInfo errorInfo)
        {
            exception.Data.Add(ErrorInfoConstants.Domain, errorInfo.Domain);
            exception.Data.Add(ErrorInfoConstants.Reason, errorInfo.Reason);
            exception.Data.Add(ErrorInfoConstants.CorrelationId, errorInfo.Metadata[ErrorInfoConstants.CorrelationId]);
        }
    }
}
