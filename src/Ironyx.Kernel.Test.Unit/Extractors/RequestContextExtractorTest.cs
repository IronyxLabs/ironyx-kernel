using Ironyx.Kernel.Execution.Contexts;
using Ironyx.Kernel.Extractors;
using Ironyx.Kernel.Generators;
using Ironyx.Kernel.Test.Unit.Fakers;
using Ironyx.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Extractors
{
    public class RequestContextExtractorTest
    {
        private ILogger<RequestContextExtractor> _logger;
        private RequestContext _requestContext = null!;
        private Mock<IUlidGenerator> _generatorMock = null!;

        public RequestContextExtractorTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<RequestContextExtractor>();
        }

        private RequestContextExtractor CreateSUT()
        {
            _requestContext = new RequestContext();
            _generatorMock = new Mock<IUlidGenerator>();

            return new RequestContextExtractor(_requestContext, _generatorMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][RCE-001] - Generate Request Id")]
        [Feature("GRE", "gRPC Endpoint")]
        public async Task RequestContextExtractor_ExtractAsync_GenerateRequestId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            _generatorMock.Setup(g => g.Get()).Returns(id);

            // Act
            await sut.ExtractAsync(new MetadataFaker().Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.RequestId);
        }

        [Fact(DisplayName = "[UNIT][RCE-002] - Get Correlation Id")]
        [Feature("GRE", "gRPC Endpoint")]
        public async Task RequestContextExtractor_ExtractAsync_GetCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            // Act
            await sut.ExtractAsync(new MetadataFaker().WithCorrelationId(id).Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.CorrelationId);
        }

        [Fact(DisplayName = "[UNIT][RCE-003] - Generate Correlation Id")]
        [Feature("GRE", "gRPC Endpoint")]
        public async Task RequestContextExtractor_ExtractAsync_GenerateCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            _generatorMock.Setup(g => g.Get()).Returns(id);

            // Act
            await sut.ExtractAsync(new MetadataFaker().Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.CorrelationId);
        }

        [Fact(DisplayName = "[UNIT][RCE-004] - Get Causation Id")]
        [Feature("GRE", "gRPC Endpoint")]
        public async Task RequestContextExtractor_ExtractAsync_GetCausationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            // Act
            await sut.ExtractAsync(new MetadataFaker().WithCausationId(id).Generate(), default);

            // Assert
            Assert.Equal(id, _requestContext.CausationId);
        }

        [Fact(DisplayName = "[UNIT][RCE-005] - Causation Id not Found")]
        [Feature("GRE", "gRPC Endpoint")]
        public async Task RequestContextExtractor_ExtractAsync_CausationIdNotFound()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            await sut.ExtractAsync(new MetadataFaker().Generate(), default);

            // Assert
            Assert.Null(_requestContext.CausationId);
        }
    }
}
