using Grpc.Core;
using Grpc.Core.Interceptors;
using Ironyx.Kernel.Execution.Constants;

namespace Ironyx.Kernel.Interceptors
{
    public class ErrorHandlingInterceptor : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (BusinessRuleException exception)
            {
                throw RpcExceptions.BusinessRule(exception);
            }
            catch (NotFoundException exception)
            {
                throw RpcExceptions.NotFound(exception);
            }
            catch (ConflictException exception)
            {
                throw RpcExceptions.Conflict(exception);
            }
            catch (Exception exception)
            {
                throw RpcExceptions.InternalServerError(exception);
            }
        }
    }

    file static class RpcExceptions
    {
        public static RpcException InternalServerError(Exception exception) => new(new Status(StatusCode.Internal, "An internal server error occured", exception));
        public static RpcException NotFound(NotFoundException exception) => new(new Status(StatusCode.NotFound, exception.Message, exception));
        public static RpcException Conflict(ConflictException exception) => new(new Status(StatusCode.AlreadyExists, exception.Message, exception));
        public static RpcException BusinessRule(BusinessRuleException exception) => new(new Status(StatusCode.InvalidArgument, exception.Message, exception), new Metadata().SetErrorCode(exception.ErrorCode));
    }

    file static class ErrorHandlingInterceptorExtensions
    {
        public static Metadata SetErrorCode(this Metadata metadata, string? errorCode)
        {
            metadata.Add(GrpcConstants.ErrorCode, errorCode ?? nameof(StatusCode.InvalidArgument));

            return metadata;
        }
    }
}
