using AutoBogus;
using FluentValidation;
using FluentValidation.Results;
using Google.Protobuf.WellKnownTypes;
using Google.Rpc;
using Grpc.Core;
using Ironyx.Kernel.Execution.Constants;
using Ironyx.Kernel.Interceptors;
using Ironyx.Kernel.Test.Unit.Kernel.Fakers;

namespace Ironyx.Kernel.Test.Unit.Kernel.Interceptors
{
    public class ErrorHandlingInterceptorTest
    {
        private ErrorHandlingInterceptor CreateSUT()
        {
            return new ErrorHandlingInterceptor();
        }

        [Fact(DisplayName = "[UNIT][EHI-001]: Handle Internal Server Error")]
        [ErrorHandlingFeature]
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleInternalServerError()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<RpcException>(async () => await sut.UnaryServerHandler(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend(), new UnaryServerMethodFaker().InternalServerError()));
            var status = result.GetRpcStatus();
            Assert.Equal((int)StatusCode.Internal, status!.Code);
            Assert.Equal("An internal server error occured", status.Message);
        }

        [Fact(DisplayName = "[UNIT][EHI-002]: Handle Not Found")]
        [ErrorHandlingFeature]
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleNotFoundError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<NotFoundException>().Generate();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<RpcException>(async () => await sut.UnaryServerHandler(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend(), new UnaryServerMethodFaker().NotFound(exception)));
            var status = result.GetRpcStatus();
            Assert.Equal((int)StatusCode.NotFound, status!.Code);
            Assert.Equal(exception.Message, status.Message);
        }

        [Fact(DisplayName = "[UNIT][EHI-003]: Handle Conflict")]
        [ErrorHandlingFeature]
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleConflict()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<ConflictException>().Generate();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<RpcException>(async () => await sut.UnaryServerHandler(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend(), new UnaryServerMethodFaker().Conflict(exception)));
            var status = result.GetRpcStatus();
            Assert.Equal((int)StatusCode.AlreadyExists, status!.Code);
            Assert.Equal(exception.Message, result.Status.Detail);
        }

        [Fact(DisplayName = "[UNIT][EHI-004]: Handle Business Rule Error")]
        [ErrorHandlingFeature]
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleBusinessRuleError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<BusinessRuleException>().Generate();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<RpcException>(async () => await sut.UnaryServerHandler(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend(), new UnaryServerMethodFaker().BusinessRule(exception)));
            var status = result.GetRpcStatus();
            Assert.Equal((int)StatusCode.FailedPrecondition, status!.Code);
            Assert.Equal(exception.Message, status.Message);
            Assert.Single(status.Details, exception.ToDetails());
        }

        [Fact(DisplayName = "[UNIT][EHI-005]: Handle Validation Error")]
        [ErrorHandlingFeature]
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleValidationError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<ValidationException>().Generate();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<RpcException>(async () => await sut.UnaryServerHandler(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend(), new UnaryServerMethodFaker().Validation(exception)));
            var status = result.GetRpcStatus()!;
            Assert.Equal((int)StatusCode.InvalidArgument, status.Code);
            Assert.Equal("VALIDATION_FAILURE", status.Message);
            Assert.Single(status.Details, exception.Errors.ToDetails());
        }
    }

    file static class ErrorHandlingInterceptorExtensions
    {
        public static Any ToDetails(this BusinessRuleException exception)
        {
            var result = new ErrorInfo
            {
                Reason = exception.ErrorCode
            };

            return Any.Pack(result);
        }

        public static Any ToDetails(this IEnumerable<ValidationFailure> failures)
        {
            var result = new BadRequest();
            foreach (var failure in failures)
            {
                result.FieldViolations.Add(new BadRequest.Types.FieldViolation { Field = failure.PropertyName, Description = failure.ErrorCode });
            }

            return Any.Pack(result);
        }

        public static BadRequest? Unpack(this Any field)
        {
            if (!field.Is(BadRequest.Descriptor)) return null;
            return field.Unpack<BadRequest>();
        }

        public static string? GetErrorCode(this Metadata metadata)
        {
            return metadata.FirstOrDefault(m => m.Key == GrpcConstants.ErrorCode)?.Value;
        }
    }
}
