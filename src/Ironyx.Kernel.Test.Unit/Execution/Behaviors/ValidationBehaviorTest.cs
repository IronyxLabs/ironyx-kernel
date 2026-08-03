using FluentValidation;
using Ironyx.Kernel.Execution.Behaviors;
using Ironyx.Kernel.Test.Unit.Execution.Fakers;
using Ironyx.Kernel.Test.Unit.Execution.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Ironyx.Kernel.Test.Unit.Execution.Behaviors
{
    public class ValidationBehaviorTest
    {
        private readonly ILogger<ValidationBehavior<TestCommand>> _logger;
        private Mock<IValidator<TestCommand>> _validatorMock = null!;

        public ValidationBehaviorTest(ITestOutputHelper outputHelper)
        {
            _logger = new LoggerFactory()
                          .AddXUnit(outputHelper)
                          .CreateLogger<ValidationBehavior<TestCommand>>();
        }

        private ValidationBehavior<TestCommand> CreateSUT()
        {
            _validatorMock = new Mock<IValidator<TestCommand>>();

            var services = new ServiceCollection();
            services.AddSingleton(_validatorMock.Object);

            return new ValidationBehavior<TestCommand>(services.BuildServiceProvider(), _logger);
        }

        [Fact(DisplayName = "[UNIT][VLB-001]: Validate Request")]
        [ErrorHandlingFeature]
        public async Task ValidationBehavior_ValidateAsync_ValidateRequest()
        {
            // Arrange
            var sut = CreateSUT();
            var command = new CommandFaker().Generate();

            // Act
            await sut.HandleAsync(command, default);

            // Assert
            _validatorMock.Verify(v => v.ValidateAsync(It.Is<ValidationContext<TestCommand>>(c => c.ThrowOnFailures && c.InstanceToValidate == command), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
