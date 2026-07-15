using AutoBogus;
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
        private Mock<IUnwrapper> _unwrapperMock = null!;
        private Mock<IRequestContext> _requestContextMock = null!;
        private Mock<IUlidGenerator> _generatorMock = null!;

        public RequestContextUnwrapperTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<RequestContextUnwrapper>();
        }

        private RequestContextUnwrapper CreateSUT()
        {
            _unwrapperMock = new Mock<IUnwrapper>();
            _requestContextMock = new Mock<IRequestContext>();
            _generatorMock = new Mock<IUlidGenerator>();

            return new RequestContextUnwrapper(_unwrapperMock.Object, _requestContextMock.Object, _generatorMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][RCU-001]: Set Request Id")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestContextUnwrapper_UnwrapAsync_SetRequestId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            _generatorMock.Setup(g => g.Get()).Returns(id);

            // Act
            await sut.UnwrapAsync(RequestFaker.Create(new AutoFaker<TestCommand>().Generate()), new MetadataFaker().Generate(), default);

            // Assert
            _requestContextMock.VerifySet(c => c.RequestId = id);
        }

        [Fact(DisplayName = "[UNIT][RCU-002]: Set Causation Id")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestContextUnwrapper_UnwrapAsync_SetCausationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            // Act
            await sut.UnwrapAsync(RequestFaker.Create(new AutoFaker<TestCommand>().Generate()), new MetadataFaker().WithCausationId(id).Generate(), default);

            // Assert
            _requestContextMock.VerifySet(c => c.CausationId = id);
        }

        [Fact(DisplayName = "[UNIT][RCU-003]: Causation Id is not Found")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestContextUnwrapper_UnwrapAsync_CausationIdIsNotFound()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            await sut.UnwrapAsync(RequestFaker.Create(new AutoFaker<TestCommand>().Generate()), new MetadataFaker().Generate(), default);

            // Assert
            _requestContextMock.VerifySet(c => c.CausationId = null);
        }

        [Fact(DisplayName = "[UNIT][RCU-004]: Set Correlation Id")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestContextUnwrapper_UnwrapAsync_SetCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            // Act
            await sut.UnwrapAsync(RequestFaker.Create(new AutoFaker<TestCommand>().Generate()), new MetadataFaker().WithCorrelationId(id).Generate(), default);

            // Assert
            _requestContextMock.VerifySet(c => c.CorrelationId = id);
        }

        [Fact(DisplayName = "[UNIT][RCU-005]: Generate Correlation Id")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestContextUnwrapper_UnwrapAsync_GenerateCorrelationId()
        {
            // Arrange
            var sut = CreateSUT();
            var id = Ulid.NewUlid();

            _generatorMock.SetupSequence(g => g.Get())
                .Returns(Ulid.NewUlid())
                .Returns(id);

            // Act
            await sut.UnwrapAsync(RequestFaker.Create(new AutoFaker<TestCommand>().Generate()), new MetadataFaker().Generate(), default);

            // Assert
            _requestContextMock.VerifySet(c => c.CorrelationId = id);
        }

        [Fact(DisplayName = "[UNIT][RCU-006]: Continue Unwrapping")]
        [Feature("CMD", "Command Handling")]
        public async Task RequestContextUnwrapper_UnwrapAsync_ContinueUnwrapping()
        {
            // Arrange
            var sut = CreateSUT();
            var request = RequestFaker.Create(new AutoFaker<TestCommand>().Generate());
            var metadata = new MetadataFaker().Generate();

            // Act
            await sut.UnwrapAsync(request, metadata, default);

            // Assert
            _unwrapperMock.Verify(uw => uw.UnwrapAsync(request, metadata, It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    file record TestCommand : Command { }
}
