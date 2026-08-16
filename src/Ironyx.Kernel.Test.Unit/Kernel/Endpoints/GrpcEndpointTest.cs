using AutoBogus;
using Google.Rpc;
using Grpc.Core;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Options;
using Ironyx.Kernel.Receivers;
using Ironyx.Kernel.Serializers;
using Ironyx.Kernel.Test.Unit.Kernel.Fakers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
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
        private Mock<IOptionsMonitor<ServiceOptions>> _optionsMock = null!;

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
            _optionsMock = new Mock<IOptionsMonitor<ServiceOptions>>();

            return new GrpcEndpoint(_deserializerMock.Object, _extractorMock.Object, _requestContextMock.Object, _commandDispatcherMock.Object, _queryDispatcherMock.Object, _optionsMock.Object, _logger);
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

        [Fact(DisplayName = "[UNIT][GRE-003]: Receving Query")]
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
            Assert.Equal(new Reply() { Data = JsonSerializer.Serialize(result) }, reply);
        }

        [Fact(DisplayName = "[UNIT][GRE-004]: Handling Internal Errror (Command)")]
        [GrpcEndpointFeature]
        public async Task GrpcEndpoint_SendAsync_HandlingInternalError()
        {
            // Arrange
            var sut = CreateSUT();
            var exception = new AutoFaker<Exception>().Generate();
            var correlationId = Ulid.NewUlid();
            var options = new AutoFaker<ServiceOptions>().Generate();

            _requestContextMock.SetupGet(rca => rca.CorrelationId).Returns(correlationId);
            _deserializerMock.Setup().ReturnsAsync(new AutoFaker<TestCommand>().Generate());
            _commandDispatcherMock.Setup().ThrowsAsync(exception);
            _optionsMock.SetupGet(o => o.CurrentValue).Returns(options);

            // Act
            // Assert
            RpcException result = await Assert.ThrowsAsync<RpcException>(async () => await sut.SendAsync(new EnvelopFaker().Generate(), ServerCallContextFaker.CreateSend()));
            GrpcEndpointAssert.InternalError(result);
            GrpcEndpointAssert.ErrorInfo(result, options.Name, correlationId);
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

    file static class GrpcEndpointAssert
    {
        public static void InternalError(RpcException exception)
        {
            var status = exception.GetRpcStatus();

            Assert.Equal((int)StatusCode.Internal, status!.Code);
            Assert.Equal("An internal server error occured", status.Message);
        }

        public static void ErrorInfo(RpcException exception, string domain, Ulid correlationId)
        {
            var errorInfo = exception.GetRpcStatus()!.GetDetail<ErrorInfo>();

            Assert.Equal(domain, errorInfo.Domain);
            Assert.Equal("INTERNAL_SERVER_ERROR", errorInfo.Reason);
            Assert.Equal(correlationId, Ulid.Parse(errorInfo.Metadata["Ironyx.ErrorInfo.CorrelationId"]));
        }
    }

    file static class GrpcEndpointTestExtensions
    {
        public static ISetup<IRequestDeserializer, Task<dynamic>> Setup(this Mock<IRequestDeserializer> mock)
        {
            return mock.Setup(d => d.DeserializeAsync(It.IsAny<Envelop>(), It.IsAny<CancellationToken>()));
        }

        public static ISetup<ICommandDispatcher, Task> Setup(this Mock<ICommandDispatcher> mock)
        {
            return mock.Setup(d => d.DispatchAsync(It.IsAny<TestCommand>(), It.IsAny<CancellationToken>()));
        }
    }
}
