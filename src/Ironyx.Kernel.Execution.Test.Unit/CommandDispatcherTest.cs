using AutoBogus;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Test.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Execution.Test.Unit
{
    public class CommandDispatcherTest
    {
        private ILogger<CommandDispatcher> _logger;

        public CommandDispatcherTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<CommandDispatcher>();
        }

        private CommandDispatcher CreateSUT(Action action)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ICommandHandler<TestCommand>>(new TestCommandHandler(action));

            return new CommandDispatcher(serviceCollection.BuildServiceProvider(), _logger);
        }

        private CommandDispatcher CreateSUT()
        {
            return new CommandDispatcher(new ServiceCollection().BuildServiceProvider(), _logger);
        }

        [Fact(DisplayName = "[UNIT][CMD-001]: Dispatch Command")]
        [CommandHandlingFeature]
        public async Task CommandDispatcher_DispatchAsync_DispatchCommand()
        {
            // Arrange
            var called = false;
            var sut = CreateSUT(() => called = true);

            // Act
            await sut.DispatchAsync(new AutoFaker<TestCommand>().Generate(), default);

            // Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "[UNIT][CMD-002]: Handler not Found")]
        [CommandHandlingFeature]
        public async Task CommandDispatcher_DispatchAsync_HandlerNotFound()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await sut.DispatchAsync(new AutoFaker<TestCommand>().Generate(), default));
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
