using Grpc.Core;
using Grpc.Core.Interceptors;

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
            catch (Exception exception)
            {
                throw RpcExceptions.InternalServerError(exception);
            }
        }
    }

    file static class RpcExceptions
    {
        public static RpcException InternalServerError(Exception exception) => new RpcException(new Status(StatusCode.Internal, "An internal server error occured", exception));
    }
}
