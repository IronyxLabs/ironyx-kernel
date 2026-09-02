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
                throw exception.ToRpcException();
            }
            catch (NotFoundException exception)
            {
                throw exception.ToRpcException();
            }
            catch (ConflictException exception)
            {
                throw exception.ToRpcException();
            }
            catch (ValidationException exception)
            {
                throw exception.ToRpcException();
            }
            catch (Exception exception)
            {
                throw exception.ToRpcException();
            }
        }
    }

    file static class ErrorHandlingInterceptorExtensions
    {
        public static RpcException ToRpcException(this BusinessRuleException exception)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.FailedPrecondition,
                Message = exception.Message
            };
            status.Details.Add(Any.Pack(new ErrorInfo
            {
                Reason = exception.ErrorCode,
            }));

            return status.ToRpcException();
        }

        public static RpcException ToRpcException(this ValidationException exception)
        {
            var error = new BadRequest();
            error.FieldViolations.AddRange(exception.Errors.ToFieldViolations());

            var status = new Google.Rpc.Status
            {
                Code = (int)Code.InvalidArgument,
                Message = "VALIDATION_FAILURE"
            };
            status.Details.Add(Any.Pack(error));

            return status.ToRpcException();
        }

        public static RpcException ToRpcException(this ConflictException exception)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.AlreadyExists,
                Message = exception.Message
            };

            return status.ToRpcException();
        }

        public static RpcException ToRpcException(this NotFoundException exception)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.NotFound,
                Message = exception.Message
            };

            return status.ToRpcException();
        }

        public static RpcException ToRpcException(this Exception exception)
        {
            var status = new Google.Rpc.Status
            {
                Code = (int)Code.Internal,
                Message = "An internal server error occured"
            };

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
