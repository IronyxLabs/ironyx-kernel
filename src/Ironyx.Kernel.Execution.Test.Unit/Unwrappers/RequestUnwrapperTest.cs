using AutoBogus;
using Ironyx.Kernel.Execution.Test.Unit.Fakers;
using Ironyx.Kernel.Execution.Test.Unit.Helpers;
using Ironyx.Kernel.Unwrappers;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Execution.Test.Unit.Unwrappers
{
    public class RequestUnwrapperTest
    {

        private ILogger<RequestUnwrapper> _logger;

        public RequestUnwrapperTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<RequestUnwrapper>();
        }

        private RequestUnwrapper CreateSUT()
        {
            return new RequestUnwrapper(_logger);
        }

        [Fact(DisplayName = "[UNIT][RQU-001]: Unwrap Request")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestUnwrapper_UnwrapAsync_UnwrapRequest()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();
            var context = ServerCallContextFaker.CreateSend<TestCommand>();

            // Act
            var result = await sut.UnwrapAsync(RequestFaker.Create(command), context.RequestHeaders, default);

            // Assert
            Assert.Equal(command, result);
        }

        [Fact(DisplayName = "[UNIT][RQU-002]: Request Type is not Defined")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestUnwrapper_UnwrapAsync_RequestTypeIsNotDefined()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();
            var context = ServerCallContextFaker.CreateSend();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.UnwrapAsync(RequestFaker.Create(command), context.RequestHeaders, default));
        }

        [Fact(DisplayName = "[UNIT][RQU-002]: Unknow Type is Defined")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestUnwrapper_UnwrapAsync_UnknownTypeIsDefined()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();
            var context = ServerCallContextFaker.CreateSend("UnknownType");

            // Act
            // Assert
            await Assert.ThrowsAsync<NotSupportedException>(async () => await sut.UnwrapAsync(RequestFaker.Create(command), context.RequestHeaders, default));
        }
    }

    file record TestCommand : Command
    {
        public string Message { get; init; } = null!;
    }
}
