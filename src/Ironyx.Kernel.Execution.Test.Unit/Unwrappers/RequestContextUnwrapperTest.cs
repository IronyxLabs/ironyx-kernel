using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Execution.Test.Unit.Fakers;
using Ironyx.Kernel.Execution.Test.Unit.Helpers;
using Ironyx.Kernel.Generators;
using Ironyx.Kernel.Unwrappers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Execution.Test.Unit.Unwrappers
{
    public class RequestContextUnwrapperTest
    {
        private ILogger<RequestContextUnwrapper> _logger;
        private RequestContext _requestContext = null!;
        private Mock<IUlidGenerator> _generatorMock = null!;

        public RequestContextUnwrapperTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<RequestContextUnwrapper>();
        }

        private RequestContextUnwrapper CreateSUT()
        {
            _requestContext = new RequestContext();
            _generatorMock = new Mock<IUlidGenerator>();

            return new RequestContextUnwrapper(_requestContext, _generatorMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][RCU-001] - Generate Request Id")]
        [Feature("GRE", "GRPC Endpoint")]
        public async Task RequestContextUnwrapper_UnwrapAsync_GenerateRequestId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            _generatorMock.Setup(g => g.Get()).Returns(id);

            // Act
            await sut.UnwrapAsync(new MetadataFaker().Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.RequestId);
        }

        [Fact(DisplayName = "[UNIT][RCU-002] - Get Correlation Id")]
        [Feature("GRE", "GRPC Endpoint")]
        public async Task RequestContextUnwrapper_UnwrapAsync_GetCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            // Act
            await sut.UnwrapAsync(new MetadataFaker().WithCorrelationId(id).Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.CorrelationId);
        }

        [Fact(DisplayName = "[UNIT][RCU-003] - Generate Correlation Id")]
        [Feature("GRE", "GRPC Endpoint")]
        public async Task RequestContextUnwrapper_UnwrapAsync_GenerateCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            _generatorMock.Setup(g => g.Get()).Returns(id);

            // Act
            await sut.UnwrapAsync(new MetadataFaker().Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.CorrelationId);
        }

        [Fact(DisplayName = "[UNIT][RCU-004] - Get Causation Id")]
        [Feature("GRE", "GRPC Endpoint")]
        public async Task RequestContextUnwrapper_UnwrapAsync_GetCausationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            // Act
            await sut.UnwrapAsync(new MetadataFaker().WithCausationId(id).Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.CausationId);
        }

        [Fact(DisplayName = "[UNIT][RCU-005] - Causation Id not Found")]
        [Feature("GRE", "GRPC Endpoint")]
        public async Task RequestContextUnwrapper_UnwrapAsync_CausationIdNotFound()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            await sut.UnwrapAsync(new MetadataFaker().Generate(), default);

            // Assert
            Assert.Null(_requestContext.CausationId);
        }
    }
}
