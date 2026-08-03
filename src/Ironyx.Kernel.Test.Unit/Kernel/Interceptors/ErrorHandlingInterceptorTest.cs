using AutoBogus;
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
            Assert.Equal(StatusCode.Internal, result.StatusCode);
            Assert.Equal(StatusCode.Internal, result.Status.StatusCode);
            Assert.Equal("An internal server error occured", result.Status.Detail);
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
            Assert.Equal(StatusCode.NotFound, result.StatusCode);
            Assert.Equal(StatusCode.NotFound, result.Status.StatusCode);
            Assert.Equal(exception.Message, result.Status.Detail);
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
            Assert.Equal(StatusCode.AlreadyExists, result.StatusCode);
            Assert.Equal(StatusCode.AlreadyExists, result.Status.StatusCode);
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
            Assert.Equal(StatusCode.InvalidArgument, result.StatusCode);
            Assert.Equal(StatusCode.InvalidArgument, result.Status.StatusCode);
            Assert.Equal(exception.Message, result.Status.Detail);
            Assert.Equal(exception.ErrorCode, result.Trailers.GetErrorCode());
        }

        [Fact(DisplayName = "[UNIT][EHI-005]: Handle Business Rule Error without Error Code")]
        [ErrorHandlingFeature]
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleBusinessRuleErrorWithoutErrorCode()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<BusinessRuleException>().Ignore(e => e.ErrorCode).Generate();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<RpcException>(async () => await sut.UnaryServerHandler(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend(), new UnaryServerMethodFaker().BusinessRule(exception)));
            Assert.Equal(StatusCode.InvalidArgument, result.StatusCode);
            Assert.Equal(StatusCode.InvalidArgument, result.Status.StatusCode);
            Assert.Equal(exception.Message, result.Status.Detail);
            Assert.Equal(nameof(StatusCode.InvalidArgument), result.Trailers.GetErrorCode());
        }
    }

    file static class ErrorHandlingInterceptorExtensions
    {
        public static string? GetErrorCode(this Metadata metadata)
        {
            return metadata.FirstOrDefault(m => m.Key == GrpcConstants.ErrorCode)?.Value;
        }
    }
}
