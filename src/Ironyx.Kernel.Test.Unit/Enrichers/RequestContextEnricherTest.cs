using Grpc.Core;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Test.Features;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Enrichers
{
    public class RequestContextEnricherTest
    {

        private ILogger<RequestContextEnricher> _logger;
        private Mock<IRequestContextAccessor> _requestContextMock;

        public RequestContextEnricherTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<RequestContextEnricher>();
        }

        private RequestContextEnricher CreateSUT()
        {
            _requestContextMock = new Mock<IRequestContextAccessor>();

            return new RequestContextEnricher(_requestContextMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][RCE-001]: Set CorrelationId")]
        [RequestSendingFeature]
        public async Task RequestContextEnricher_EnrichAsync_SetCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var metadata = new Metadata();
            var correlationId = Ulid.NewUlid();

            _requestContextMock.SetupGet(rc => rc.CorrelationId).Returns(correlationId);

            // Act
            await sut.EnrichAsync(metadata, default);

            // Assert
            Assert.Single(metadata, m => m.Key == "correlation-id" && Ulid.Parse(m.Value).Equals(correlationId));
        }

        [Fact(DisplayName = "[UNIT][RCE-002]: Set CausationId")]
        [RequestSendingFeature]
        public async Task RequestContextEnricher_EnrichAsync_SetCausationId()
        {
            // Arrange
            var sut = CreateSUT();
            var metadata = new Metadata();
            var requestId = Ulid.NewUlid();

            _requestContextMock.SetupGet(rc => rc.RequestId).Returns(requestId);

            // Act
            await sut.EnrichAsync(metadata, default);

            // Assert
            Assert.Single(metadata, m => m.Key == "causation-id" && Ulid.Parse(m.Value).Equals(requestId));
        }
    }
}
