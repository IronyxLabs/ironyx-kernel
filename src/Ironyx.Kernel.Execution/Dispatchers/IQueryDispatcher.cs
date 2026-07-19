namespace Ironyx.Kernel.Execution.Dispatchers
{
    public interface IQueryDispatcher
    {
        Task<TResult> DispatchAsync<TResult>(Query<TResult> query, CancellationToken cancellationToken);
    }
}
