namespace Ironyx.Kernel.Sample.Handlers
{
    public record SampleCommand : Command
    {
        public string Message { get; init; } = null!;
    }

    public class SampleCommandHandler : ICommandHandler<SampleCommand>
    {
        public Task HandleAsync(SampleCommand command, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Message received: {command.Message}");

            return Task.CompletedTask;
        }
    }
}
