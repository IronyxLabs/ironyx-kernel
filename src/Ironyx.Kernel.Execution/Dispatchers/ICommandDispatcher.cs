namespace Ironyx.Kernel.Execution.Dispatchers
{
    public interface ICommandDispatcher
    {
        Task DispatchAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : Command;
    }
}
