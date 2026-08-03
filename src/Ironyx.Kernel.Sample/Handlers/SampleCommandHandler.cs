using FluentValidation;
using Ironyx.Kernel.Execution.Senders;

namespace Ironyx.Kernel.Sample.Handlers
{
    [RequestVersion("v1")]
    public record SampleCommand : Command
    {
        public string Message { get; init; } = null!;
        public EventResetMode TestEnum { get; init; }
    }

    public class SampleCommandValidator : AbstractValidator<SampleCommand>
    {
        public SampleCommandValidator()
        {
            RuleFor(c => c.Message).NotEqual("INVALID_INPUT");
        }
    }

    public class SampleCommandHandler : ICommandHandler<SampleCommand>
    {
        private readonly IRequestSender _sender;

        public SampleCommandHandler(IRequestSender sender)
        {
            _sender = sender;
        }

        public async Task HandleAsync(SampleCommand command, CancellationToken cancellationToken = default)
        {
            if (command.Message == "FORWARD") await _sender.SendAsync(new SampleCommand { Message = "Hello from CommandSender!", TestEnum = command.TestEnum }, cancellationToken);

            Console.WriteLine($"Message received: {command.Message} (TestEnum: {command.TestEnum})");
            throw new BusinessRuleException("BUSINESS_001", "Test Business Exception");
        }
    }
}
