using Grpc.Core;
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
        public async Task ErrorHandlingIntercepter_UnaryServerHandle_HandleInternalServerErrror()
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
    }
}
