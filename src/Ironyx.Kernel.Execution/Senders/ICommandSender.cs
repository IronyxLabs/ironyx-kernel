namespace Ironyx.Kernel.Execution.Senders
{
    public interface ICommandSender
    {
        Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : Command;
    }
}
