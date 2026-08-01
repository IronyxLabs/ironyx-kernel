using AutoBogus;
using Ironyx.Kernel.Registry;
using Ironyx.Kernel.Serializers;
using Ironyx.Kernel.Test.Unit.Kernel.Fakers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Serializers
{
    public class RequestDeserializerTest
    {

        private readonly ILogger<RequestDeserializer> _logger;
        private Mock<IRuntimeTypeResolver> _typeResolverMock = null!;

        public RequestDeserializerTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<RequestDeserializer>();
        }

        private RequestDeserializer CreateSUT()
        {
            _typeResolverMock = new Mock<IRuntimeTypeResolver>();

            return new RequestDeserializer(_typeResolverMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][RGQ-001]: Deserialize Request")]
        [GrpcEndpointFeature]
        public async Task RequestDeserializer_DeserializeAsync_DeserializeRequest()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();

            _typeResolverMock.SetupGet(tr => tr[command.GetType().FullName!, "v1"]).Returns(command.GetType());

            // Act
            var result = await sut.DeserializeAsync(new EnvelopFaker().Use(command).Generate(), default);

            // Assert
            Assert.Equal(command, result);
        }

        [Fact(DisplayName = "[UNIT][RGQ-002]: Request Type is not defined")]
        [GrpcEndpointFeature]
        public async Task RequestDeserializer_DeserializeAsync_RequestTypeIsNotDefined()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.DeserializeAsync(new EnvelopFaker().Use(command).WithoutType().Generate(), default));
        }

        [Fact(DisplayName = "[UNIT][RGQ-003]: Version is not defined")]
        [GrpcEndpointFeature]
        public async Task RequestDeserializer_DeserializeAsync_VersionIsNotDefined()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await sut.DeserializeAsync(new EnvelopFaker().Use(command).WithoutVersion().Generate(), default));
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command
    {
        public required string Message { get; init; }
    }
}
