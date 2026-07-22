namespace Ironyx.Kernel.Execution
{
    public interface IQueryHandler<TQuery, TResult>
        where TQuery : Query<TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
