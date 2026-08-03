using AutoBogus;
using Ironyx.Kernel.Execution.Behaviors;
using Ironyx.Kernel.Execution.Dispatchers;
using Ironyx.Kernel.Execution.Registries;
using Ironyx.Kernel.Test.Unit.Execution.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

[assembly: InternalsVisibleTo("Ironyx.Kernel.Execution")]
namespace Ironyx.Kernel.Test.Unit.Execution
{
    public class CommandDispatcherTest
    {
        private readonly ILogger<CommandDispatcher> _logger;
        private Mock<IHandlerTypeResolver> _resolverMock = null!;
        private Mock<IPreHandler<TestCommand>> _preHandlerMock = null!;

        public CommandDispatcherTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<CommandDispatcher>();
        }

        private CommandDispatcher CreateSUT(Action action, Action? preAction = null)
        {
            _resolverMock = new Mock<IHandlerTypeResolver>();
            _preHandlerMock = new Mock<IPreHandler<TestCommand>>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new TestCommandHandler(action));
            if (preAction is not null) serviceCollection.AddSingleton(new PreTestCommandHandler(preAction));

            return new CommandDispatcher(serviceCollection.BuildServiceProvider(), _resolverMock.Object, _logger);
        }

        [Fact(DisplayName = "[UNIT][CMD-001]: Dispatch Command")]
        [CommandHandlingFeature]
        public async Task CommandDispatcher_DispatchAsync_DispatchCommand()
        {
            // Arrange
            var called = false;
            var sut = CreateSUT(() => called = true);

            _resolverMock.SetupGet(r => r[typeof(TestCommand)]).Returns(new HandlerTypeDescription { Handler = typeof(TestCommandHandler) });

            // Act
            await sut.DispatchAsync(new AutoFaker<TestCommand>().Generate(), default);

            // Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "[UNIT][CMD-002]: Use PreHandle")]
        [CommandHandlingFeature]
        public async Task CommandDispatcher_DispatchAsync_UsePreHandle()
        {
            // Arrange
            var called = false;
            var sut = CreateSUT(() => { }, () => called = true);
            var command = new AutoFaker<TestCommand>().Generate();

            _resolverMock.SetupGet(r => r[typeof(TestCommand)]).Returns(new HandlerTypeDescription
            {
                Handler = typeof(TestCommandHandler),
                PreHandlers = [typeof(PreTestCommandHandler)]
            });

            // Act
            await sut.DispatchAsync(command, default);

            // Assert
            Assert.True(called);
        }
    }

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

    file class PreTestCommandHandler : IPreHandler<TestCommand>
    {
        private readonly Action _action;

        public PreTestCommandHandler(Action action)
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
