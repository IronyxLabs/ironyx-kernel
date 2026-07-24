namespace Ironyx.Kernel.Execution.Senders
{
    public interface IRequestSender
    {
        Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : Command;

        Task<TResult?> GetAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken)
            where TQuery : Query<TResult>;
    }
}
