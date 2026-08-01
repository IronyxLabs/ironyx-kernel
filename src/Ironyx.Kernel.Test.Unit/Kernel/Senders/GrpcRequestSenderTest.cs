using AutoBogus;
using Grpc.Core;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Senders;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit.Abstractions;

[assembly: InternalsVisibleTo("Ironyx.Kernel")]
namespace Ironyx.Kernel.Test.Unit.Kernel.Senders
{
    public class GrpcRequestSenderTest
    {

        private readonly ILogger<GrpcRequestSender> _logger;
        private Mock<IGenericClient> _clientMock = null!;
        private Mock<IEnricher> _enricherMock = null!;

        public GrpcRequestSenderTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<GrpcRequestSender>();
        }

        private GrpcRequestSender CreateSUT()
        {
            _clientMock = new Mock<IGenericClient>();
            _enricherMock = new Mock<IEnricher>();

            return new GrpcRequestSender(_clientMock.Object, _enricherMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][GRS-001]: Send Command")]
        [RequestSendingFeature]
        public async Task GrpcRequestSender_SendAsync_SendCommand()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new AutoFaker<TestCommand>().Generate();
            var envelop = await command.AsEnvelopAsync();

            // Act
            await sut.SendAsync(command, default);

            // Assert
            _clientMock.Verify(c => c.SendAsync(envelop, It.IsAny<Metadata>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact(DisplayName = "[UNIT][GRS-002]: Version is not defined")]
        [RequestSendingFeature]
        public async Task GrpcRequestSender_SendAsync_VersionIsNotDefined()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.SendAsync(new AutoFaker<TestCommandWithoutVersion>().Generate(), default));
        }

        [Fact(DisplayName = "[UNIT][GRS-003]: Enrich Command Metadata")]
        [RequestSendingFeature]
        public async Task GrpcRequestSender_SendAsync_EnrichCommandMetadata()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            await sut.SendAsync(new AutoFaker<TestCommand>().Generate(), default);

            // Assert
            _enricherMock.Verify(e => e.EnrichAsync(It.IsAny<Metadata>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact(DisplayName = "[UNIT][GRS-004]: Send Query")]
        [RequestSendingFeature]
        public async Task GrpcRequestSender_GetAsync_SendQuery()
        {
            // Arrange
            var sut = CreateSUT();
            var query = new AutoFaker<TestQuery>().Generate();
            var envelop = await query.AsEnvelopAsync();
            var expected = new AutoFaker<TestQuery.Result>().Generate();

            _clientMock.Setup(c => c.GetAsync(It.IsAny<Envelop>(), It.IsAny<Metadata>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Reply { Data = JsonSerializer.Serialize(expected) });

            // Act
            var result = await sut.GetAsync<TestQuery, TestQuery.Result>(query, default);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "[UNIT][GRS-005]: Enrich Query Metadata")]
        [RequestSendingFeature]
        public async Task GrpcRequestSender_GetAsync_EnrichQueryMetadata()
        {
            // Arrange
            var sut = CreateSUT();

            _clientMock.Setup(c => c.GetAsync(It.IsAny<Envelop>(), It.IsAny<Metadata>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Reply { Data = JsonSerializer.Serialize(new AutoFaker<TestQuery.Result>().Generate()) });

            // Act
            await sut.GetAsync<TestQuery, TestQuery.Result>(new AutoFaker<TestQuery>().Generate(), default);

            // Assert
            _enricherMock.Verify(e => e.EnrichAsync(It.IsAny<Metadata>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command { }

    file record TestCommandWithoutVersion : Command { }

    [RequestVersion("v1")]
    file record TestQuery : Query<TestQuery.Result>
    {
        public record Result
        {
            public required string Message { get; init; }
        }
    }

    file static class GrpcRequestSenderTestExtensions
    {
        public static async Task<Envelop> AsEnvelopAsync(this TestCommand command)
        {
            await using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            await JsonSerializer.SerializeAsync(stream, command);
            stream.Position = 0;

            return new Envelop
            {
                Type = command.GetType().FullName,
                Version = command.GetType().GetCustomAttribute<RequestVersionAttribute>()!.Version,
                Payload = await reader.ReadToEndAsync()
            };
        }

        public static async Task<Envelop> AsEnvelopAsync(this TestQuery query)
        {
            await using var stream = new MemoryStream();
            using var reader = new StreamReader(stream);

            await JsonSerializer.SerializeAsync(stream, query);
            stream.Position = 0;

            return new Envelop
            {
                Type = query.GetType().FullName,
                Version = query.GetType().GetCustomAttribute<RequestVersionAttribute>()!.Version,
                Payload = await reader.ReadToEndAsync()
            };
        }
    }
}
