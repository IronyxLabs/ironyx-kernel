using Ironyx.Kernel.Execution.Registries;

namespace Ironyx.Kernel.Test.Unit.Execution.Registries
{
    // TODO: Refactor
    public class HandlerRegistryTest
    {
        private HandlerRegistry CreateSUT()
        {
            return new HandlerRegistry();
        }

        [Fact(DisplayName = "[UNIT][CHR-001]: Registrate Command")]
        [CommandHandlingFeature]
        public void CommandHandlerRegistry_Add_RegistrateCommand()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            sut.Add(typeof(TestCommand), typeof(TestCommandHandler), []);

            // Assert
            //Assert.Equal(typeof(TestCommandHandler), sut[typeof(TestCommand)]);
        }

        [Fact(DisplayName = "[UNIT][CHR-002]: Handler has already been registered")]
        [CommandHandlingFeature]
        public void CommandHandlerRegistry_Add_HandlerHasAlreadyBeenRegistered()
        {
            // Arrange
            var sut = CreateSUT();

            sut.Add(typeof(TestCommand), typeof(TestCommandHandler), []);

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => sut.Add(typeof(TestCommand), typeof(TestCommandHandler), []));
        }

        [Fact(DisplayName = "[UNIT][CHR-003]: Handler has not been registered")]
        [CommandHandlingFeature]
        public void CommandHandlerRegistry_Add_HandlerHasNotBeenRegistered()
        {
            // Arrange
            var sut = CreateSUT();

            // Act
            // Assert
            Assert.Throws<InvalidOperationException>(() => sut[typeof(TestCommand)]);
        }
    }

    file record TestCommand : Command { }

    file class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public async Task HandleAsync(TestCommand command, CancellationToken cancellationToken) => throw new NotImplementedException();
    }
}
