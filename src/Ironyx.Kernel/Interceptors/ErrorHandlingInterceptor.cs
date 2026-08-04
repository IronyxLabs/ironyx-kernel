using FluentValidation;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
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
                throw new NotImplementedException();
                //throw RpcExceptions.BusinessRule(exception);
            }
            catch (NotFoundException exception)
            {
                throw new NotImplementedException();
                //throw RpcExceptions.NotFound(exception);
            }
            catch (ConflictException exception)
            {
                throw new NotImplementedException();
                //throw RpcExceptions.Conflict(exception);
            }
            catch (ValidationException exception)
            {
                throw exception.ToRpcException();
            }
            catch (Exception exception)
            {
                throw new NotImplementedException();
                //throw RpcExceptions.InternalServerError(exception);
            }
        }
    }

    file static class ErrorHandlingInterceptorExtensions
    {
        public static RpcException ToRpcException(this ValidationException exception)
        {
            var error = new BadRequest();
            error.FieldViolations.AddRange(exception.Errors.ToFieldViolations());

            var status = new Google.Rpc.Status
            {
                Code = (int)Code.InvalidArgument,
                Message = "Validation Failure"
            };
            status.Details.Add(Any.Pack(error));

            return status.ToRpcException();
        }

        public static IEnumerable<BadRequest.Types.FieldViolation> ToFieldViolations(this IEnumerable<FluentValidation.Results.ValidationFailure> failures)
        {
            foreach (var failure in failures)
            {
                yield return new BadRequest.Types.FieldViolation { Field = failure.PropertyName, Description = failure.ErrorCode };
            }
        }

        public static Metadata SetErrorCode(this Metadata metadata, string? errorCode)
        {
            metadata.Add(GrpcConstants.ErrorCode, errorCode ?? nameof(StatusCode.InvalidArgument));

            return metadata;
        }
    }
}
