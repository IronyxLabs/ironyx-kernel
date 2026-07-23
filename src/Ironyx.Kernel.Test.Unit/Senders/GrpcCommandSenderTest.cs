using AutoBogus;
using Grpc.Core;
using Ironyx.Kernel.Enrichers;
using Ironyx.Kernel.Senders;
using Ironyx.Kernel.Test.Features;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Xunit.Abstractions;

[assembly: InternalsVisibleTo("Ironyx.Kernel")]
namespace Ironyx.Kernel.Test.Unit.Senders
{
    public class GrpcCommandSenderTest
    {

        private readonly ILogger<GrpcCommandSender> _logger;
        private Mock<IGenericClient> _clientMock = null!;
        private Mock<IEnricher> _enricherMock = null!;

        public GrpcCommandSenderTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<GrpcCommandSender>();
        }

        private GrpcCommandSender CreateSUT()
        {
            _clientMock = new Mock<IGenericClient>();
            _enricherMock = new Mock<IEnricher>();

            return new GrpcCommandSender(_clientMock.Object, _enricherMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][GRC-001]: Send Command")]
        [RequestSendingFeature]
        public async Task GrpcCommandSender_SendAsync_SendCommand()
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

        [Fact(DisplayName = "[UNIT][GRC-002]: Version is not defined")]
        [RequestSendingFeature]
        public async Task GrpcCommandSender_SendAsync_VersionIsNotDefined()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.SendAsync(new AutoFaker<TestCommandWithoutVersion>().Generate(), default));
        }

        [Fact(DisplayName = "[UNIT][GRC-003]: Enrich Metadata")]
        [RequestSendingFeature]
        public async Task GrpcCommandSender_SendAsync_EnrichMetadata()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            await sut.SendAsync(new AutoFaker<TestCommand>().Generate(), default);

            // Assert
            _enricherMock.Verify(e => e.EnrichAsync(It.IsAny<Metadata>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }

    [RequestVersion("v1")]
    file record TestCommand : Command { }

    file record TestCommandWithoutVersion : Command { }

    file static class GrpcCommandSenderTestExtensions
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
    }
}
