using AutoBogus;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Receivers;
using Ironyx.Kernel.Serializers;
using Ironyx.Kernel.Test.Features;
using Ironyx.Kernel.Test.Unit.Fakers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Endpoints
{
    public class GrpcEndpointTest
    {

        private readonly ILogger<GrpcEndpoint> _logger;

        private Mock<IRequestDeserializer> _deserializerMock = null!;
        private Mock<IExtractor> _extractorMock = null!;
        private Mock<IRequestContextAccessor> _requestContextMock = null!;
        private Mock<ICommandDispatcher> _dispatcherMock = null!;

        public GrpcEndpointTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<GrpcEndpoint>();
        }

        private GrpcEndpoint CreateSUT()
        {
            _deserializerMock = new Mock<IRequestDeserializer>();
            _extractorMock = new Mock<IExtractor>();
            _requestContextMock = new Mock<IRequestContextAccessor>();
            _dispatcherMock = new Mock<ICommandDispatcher>();

            return new GrpcEndpoint(_deserializerMock.Object, _extractorMock.Object, _requestContextMock.Object, _dispatcherMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][GRE-001]: Receiving Command")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_ReceivingCommand()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

            // Act
            await sut.SendAsync(new EnvelopFaker().Use(command).Generate(), ServerCallContextFaker.CreateSend());

            // Assert
            _dispatcherMock.Verify(d => d.DispatchAsync<Command>(It.Is<TestCommand>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "[UNIT][GRE-002]: Unwrap Request")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_UnwrapRequest()
        {
            // Arrange
            var sut = CreateSUT();
            var callContext = ServerCallContextFaker.CreateSend();
            TestCommand command = new AutoFaker<TestCommand>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

            // Act
            await sut.SendAsync(new EnvelopFaker().Use(command).Generate(), callContext);

            // Assert
            _extractorMock.Verify(u => u.ExtractAsync(callContext.RequestHeaders, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "[UNIT][GRE-003]: Accept Request")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_AcceptRequest()
        {
            // Arrange
            var sut = CreateSUT();
            var callContext = ServerCallContextFaker.CreateSend();
            TestCommand command = new AutoFaker<TestCommand>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

            // Act
            var reply = await sut.SendAsync(new EnvelopFaker().Use(command).Generate(), callContext);

            // Assert
            Assert.Equal(new Reply() { Status = "ACCEPTED" }, reply);
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command
    {
        public required string Name { get; set; }
    }
}
