namespace Ironyx.Kernel
{
    public interface ICommandHandler<in TCommand>
        where TCommand : Command
    {
        Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
