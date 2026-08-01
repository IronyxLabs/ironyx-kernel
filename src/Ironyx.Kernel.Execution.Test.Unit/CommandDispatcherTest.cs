using AutoBogus;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Execution.Registries;
using Ironyx.Kernel.Test.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

[assembly: InternalsVisibleTo("Ironyx.Kernel.Execution")]
namespace Ironyx.Kernel.Execution.Test.Unit
{
    public class CommandDispatcherTest
    {
        private ILogger<CommandDispatcher> _logger;
        private Mock<IHandlerTypeResolver> _resolverMock = null!;

        public CommandDispatcherTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<CommandDispatcher>();
        }

        private CommandDispatcher CreateSUT(Action action)
        {
            _resolverMock = new Mock<IHandlerTypeResolver>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ICommandHandler<TestCommand>>(new TestCommandHandler(action));

            return new CommandDispatcher(serviceCollection.BuildServiceProvider(), _resolverMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][CMD-001]: Dispatch Command")]
        [CommandHandlingFeature]
        public async Task CommandDispatcher_DispatchAsync_DispatchCommand()
        {
            // Arrange
            var called = false;
            var sut = CreateSUT(() => called = true);

            _resolverMock.SetupGet(r => r[typeof(TestCommand)]).Returns(typeof(ICommandHandler<TestCommand>));

            // Act
            await sut.DispatchAsync(new AutoFaker<TestCommand>().Generate(), default);

            // Assert
            Assert.True(called);
        }
    }

    file record TestCommand : Command { }

    file class TestCommandHandler : ICommandHandler<TestCommand>
    {
        private readonly Action _action;

        public TestCommandHandler(Action action)
        {
            _action = action;
        }

        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            _action();

            return Task.CompletedTask;
        }
    }
}
