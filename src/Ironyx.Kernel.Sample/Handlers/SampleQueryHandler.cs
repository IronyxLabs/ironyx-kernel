using Ironyx.Kernel.Execution;

namespace Ironyx.Kernel.Sample.Handlers
{
    [RequestVersion("v1")]
    public record SampleQuery : Query<SampleQuery.Result>
    {
        public required string Name { get; init; }

        public record Result
        {
            public required string Message { get; init; }
        }
    }

    public class SampleQueryHandler : IQueryHandler<SampleQuery, SampleQuery.Result>
    {
        public Task<SampleQuery.Result> HandleAsync(SampleQuery query, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SampleQuery.Result { Message = $"Hello {query.Name}!" });
        }
    }
}
