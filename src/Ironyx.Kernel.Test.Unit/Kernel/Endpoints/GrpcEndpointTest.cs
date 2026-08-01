using AutoBogus;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Receivers;
using Ironyx.Kernel.Serializers;
using Ironyx.Kernel.Test.Unit.Kernel.Fakers;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Kernel.Endpoints
{
    public class GrpcEndpointTest
    {

        private readonly ILogger<GrpcEndpoint> _logger;

        private Mock<IRequestDeserializer> _deserializerMock = null!;
        private Mock<IExtractor> _extractorMock = null!;
        private Mock<IRequestContextAccessor> _requestContextMock = null!;
        private Mock<ICommandDispatcher> _commandDispatcherMock = null!;
        private Mock<IQueryDispatcher> _queryDispatcherMock = null!;

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
            _commandDispatcherMock = new Mock<ICommandDispatcher>();
            _queryDispatcherMock = new Mock<IQueryDispatcher>();

            return new GrpcEndpoint(_deserializerMock.Object, _extractorMock.Object, _requestContextMock.Object, _commandDispatcherMock.Object, _queryDispatcherMock.Object, _logger);
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
            _commandDispatcherMock.Verify(d => d.DispatchAsync(It.Is<TestCommand>(c => c.Name == command.Name), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "[UNIT][GRE-002]: Extract Request")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_ExtractRequest()
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

        [Fact(DisplayName = "[UNIT][GRE-003]: Accept Command")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_AcceptCommand()
        {
            // Arrange
            var sut = CreateSUT();
            var callContext = ServerCallContextFaker.CreateSend();
            var command = new AutoFaker<TestCommand>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(command);

            // Act
            var reply = await sut.SendAsync(new EnvelopFaker().Use(command).Generate(), callContext);

            // Assert
            Assert.Equal(new Reply() { Status = "ACCEPTED" }, reply);
        }

        [Fact(DisplayName = "[UNIT][GRE-004]: Receving Query")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_GetAsync_ReceivingQuery()
        {
            // Arrange
            var sut = CreateSUT();
            var callContext = ServerCallContextFaker.CreateGet();
            var query = new AutoFaker<TestQuery>().Generate();
            var result = new AutoFaker<TestQuery.Result>().Generate();

            _deserializerMock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>())).ReturnsAsync(query);
            _queryDispatcherMock.Setup(d => d.DispatchAsync<dynamic>(It.IsAny<TestQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(result);

            // Act
            var reply = await sut.GetAsync(new EnvelopFaker().Use<TestQuery, TestQuery.Result>(query).Generate(), callContext);

            // Assert
            Assert.Equal(new Reply() { Status = "OK", Data = JsonSerializer.Serialize(result) }, reply);
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command
    {
        public required string Name { get; set; }
    }

    [RequestVersion("v1")]
    file record TestQuery : Query<TestQuery.Result>
    {
        public required string Name { get; set; }

        public record Result
        {
            public required string Message { get; init; }
        }
    }
}
